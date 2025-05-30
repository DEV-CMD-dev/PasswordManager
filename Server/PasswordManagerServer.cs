using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    public class PasswordManagerServer
    {
        string IP = ConfigurationManager.AppSettings["ip"];
        int PORT = int.Parse(ConfigurationManager.AppSettings["port"]);
        IPEndPoint server;
        TcpListener listener;
        PasswordManager_db db;

        List<ServerMessage> Waiting2FA;
        public PasswordManagerServer()
        {
            server = new IPEndPoint(IPAddress.Parse(IP), PORT);
            listener = new TcpListener(server);
            db = new PasswordManager_db();
            Waiting2FA = new List<ServerMessage>();
        }
        public async void Start()
        {
            try
            {
                listener.Start();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Server started!");
                Console.ResetColor();
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine($"client connected: {client.Client.RemoteEndPoint}");
                    var ns = client.GetStream();
                    StreamReader sr = new StreamReader(ns);
                    StreamWriter sw = new StreamWriter(ns);
                    string json = sr.ReadLine();
                    ServerMessage message = JsonSerializer.Deserialize<ServerMessage>(json);

                    if (message == null) return;

                    switch (message.Action)
                    {
                        case "Login":
                            Login(message, sw);
                            break;
                        case "Register":
                            Register(message, sw);
                            break;
                        case "AddPassword":
                            AddPassword(message, sw);
                            break;
                        case "UpdateProfile":
                            UpdateProfile(message, sw);
                            break;
                        case "2FAPassword":
                            Check2FAPassword(message, sw);
                            break;
                        case "2FASendAgain":
                            SendAgain2FAPassword(message, sw);
                            break;
                        case "GetPasswords":
                            GetPasswords(message, sw);
                            break;
                        case "ChangePassword":
                            ChangePassword(message, sw);
                            break;
                        case "RemovePassword":
                            RemovePassword(message, sw);
                            break;
                        case "AddImage":
                            AddImage(message, sw);
                            break;
                        case "RecoverPassword":
                            RecoverPassword(message, sw);
                            break;
                        case "UpdateAuthorizationData":
                            UpdateAuthorizationData(message, sw);
                            break;
                        default:
                            Console.WriteLine("Unknown action: " + message.Action);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }

        }

        private async void UpdateAuthorizationData(ServerMessage message, StreamWriter sw)
        {
            try
            {
                var password = message.Autorization_Data.FirstOrDefault();
                if (password != null)
                {
                    var passwordDb = db.Autorization_Data.Where(p => p.Id == password.Id && p.AccountId == password.AccountId).FirstOrDefault();
                    if (passwordDb != null)
                    {
                        passwordDb.Site = password.Site;
                        passwordDb.Login = password.Login;
                        passwordDb.Password = password.Password;
                        passwordDb.IsFavourite = password.IsFavourite;
                        db.SaveChanges();
                        message.Message = "OK";
                        //message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == message.Account.Id).ToList();
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                    }
                    else
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }  
        }

        private async void RecoverPassword(ServerMessage message, StreamWriter sw)
        {
            Account res = null;
            if (message.Account.Email != null)
            {
                res = db.Accounts.FirstOrDefault(a => a.Email == message.Account.Email);
                
            }
            else // username
            {
                res = db.Accounts.FirstOrDefault(a => a.Username == message.Account.Username);
            }
            if (res != null)
            {
                message.Message = "OK";
                message.Account.Id = res.Id; // fix bug with 2fa
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
                // after sending message to client, add to waiting list
                message.Account = res;
                message.Code2FA = new Random().Next(100000, 999999);
                Waiting2FA.Add(message);
                Send2FAMessage(message);
            }
            else
            {
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
        }

        private async void RemovePassword(ServerMessage message, StreamWriter sw)
        {
            try
            {
                var password = message.Autorization_Data.FirstOrDefault();
                if (password != null)
                {
                    //db.Autorization_Data.Remove(password);
                    var passwordfromDb = db.Autorization_Data.FirstOrDefault(p => p.Id == password.Id && p.AccountId == message.Account.Id);
                    if (passwordfromDb == null)
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                        return;
                    }

                    db.Autorization_Data.Remove(passwordfromDb);
                    int res = db.SaveChanges();
                    if (res > 0)
                    {
                        message.Message = "OK";
                        //message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == message.Account.Id).ToList();
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                    }
                    else
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
        }

        private async void GetPasswords(ServerMessage message, StreamWriter sw)
        {
            try
            {
                message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == message.Account.Id).ToList();
                if (message.Autorization_Data != null)
                {
                    message.Message = "OK";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
        }

        private async void SendAgain2FAPassword(ServerMessage message, StreamWriter sw)
        {
            try
            {
                message.Code2FA = new Random().Next(100000, 999999);
                var waiting_user = Waiting2FA.FirstOrDefault(w => w.Account.Id == message.Account.Id);
                if (waiting_user != null)
                {
                    Waiting2FA.Remove(waiting_user);
                    Waiting2FA.Add(message);
                    Send2FAMessage(message);
                    message.Message = "OK";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
            
        }

        private async void Check2FAPassword(ServerMessage message, StreamWriter sw)
        {
            try
            {
                var waiting_user = Waiting2FA.FirstOrDefault(w => w.Account.Id == message.Account.Id);
                if (waiting_user != null)
                {
                    if (waiting_user.Code2FA == message.Code2FA)
                    {
                        Waiting2FA.Remove(waiting_user);
                        message.Message = "OK";
                        var accountDb = db.Accounts.FirstOrDefault(a => a.Id == message.Account.Id);
                        if (accountDb != null)
                        {
                            message.Account = accountDb;
                            message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == accountDb.Id).ToList();
                            if (File.Exists("../../../Images/" + message.Account.AvatarPath))
                            {
                                message.Image = File.ReadAllBytes("../../../Images/" + message.Account.AvatarPath);
                                message.FileNameImage = message.Account.AvatarPath;
                            }
                            string json = JsonSerializer.Serialize(message);
                            await sw.WriteLineAsync(json);
                            await sw.FlushAsync();
                        }
                    }
                    else
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        await sw.WriteLineAsync(json);
                        await sw.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                sw.WriteLine(json);
                sw.Flush();
            }
        }

        private async void UpdateProfile(ServerMessage message, StreamWriter sw)
        {
            try
            {
                var accountDb = db.Accounts.FirstOrDefault(a => a.Id == message.Account.Id);
                if (accountDb != null)
                {
                    accountDb.Is2FAEnabled = message.Account.Is2FAEnabled;
                    db.SaveChanges();
                    message.Message = "OK";
                    message.Account = accountDb;
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
        }

        private async void ChangePassword(ServerMessage message, StreamWriter sw)
        {
            try
            {
                var accountDb = db.Accounts.FirstOrDefault(a => a.Id == message.Account.Id);
                if (accountDb != null)
                {
                    accountDb.Password = message.Account.Password;
                    db.SaveChanges();
                    message.Message = "OK";
                    message.Account = accountDb;
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    await sw.WriteLineAsync(json);
                    await sw.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
            
        }

        private async void AddImage(ServerMessage message, StreamWriter sw)
        {
            const string FOLDER = "../../../Images/";
            if (!Directory.Exists(FOLDER))
            {
                Directory.CreateDirectory(FOLDER);
            }
            string path = Path.Combine(FOLDER, message.FileNameImage);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            await File.WriteAllBytesAsync(path, message.Image);
            if (File.Exists(path))
            {
                message.Message = "OK";
                message.Action = "";
                var accountDb = db.Accounts.FirstOrDefault(a => a.Id == message.Account.Id);
                if (accountDb != null)
                {
                    accountDb.AvatarPath = message.FileNameImage;
                    db.SaveChanges();
                    message.Account = accountDb;
                }
                //message.Account.AvatarPath = path;
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
            else
            {
                message.Message = "ERROR";
                message.Action = "";
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
            }
        }

        private void AddPassword(ServerMessage message, StreamWriter sw)
        {
            try
            {
                db.Autorization_Data.Add(message.NewPassword);
                int res = db.SaveChanges();
                if (res > 0)
                {
                    message.Message = "OK";
                    message.NewPassword = null; // fix cycles error
                    message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == message.Account.Id).ToList();
                    string json = JsonSerializer.Serialize(message);
                    sw.WriteLine(json);
                    sw.Flush();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    sw.WriteLine(json);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                sw.WriteLine(json);
                sw.Flush();
            }
            
        }

        private void Register(ServerMessage message, StreamWriter sw)
        {
            try
            {
                Account account = message.Account;
                db.Accounts.Add(account);
                int res = db.SaveChanges();
                if (res > 0)
                {
                    message.Message = "OK";
                    string json = JsonSerializer.Serialize(message);
                    sw.WriteLine(json);
                    sw.Flush();
                }
                else
                {
                    message.Message = "ERROR";
                    string json = JsonSerializer.Serialize(message);
                    sw.WriteLine(json);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                sw.WriteLine(json);
                sw.Flush();
            }
            
        }

        public void Login(ServerMessage message, StreamWriter sw)
        {
            try
            {
                Account account = message.Account;
                string email = account.Email;
                string username = account.Username;
                string password = account.Password;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var res = db.Accounts.FirstOrDefault(a => a.Email == email && a.Password == password);
                    if (res != null)
                    {
                        if (res.Is2FAEnabled == true)
                        {
                            // save to waiting list(5 minutes)
                            message.Account = res;
                            var waiting_user = message; // send to email, if 2FA enabled
                            waiting_user.Code2FA = new Random().Next(100000, 999999);
                            Waiting2FA.Add(waiting_user);
                            Task.Run(() => Send2FAMessage(waiting_user));
                            //
                            message.Message = "2FA";
                            string json = JsonSerializer.Serialize(message);
                            sw.WriteLine(json);
                            sw.Flush();
                        }
                        else
                        {
                            message.Message = "OK";
                            message.Account = res;
                            message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == res.Id).ToList();
                            if (File.Exists("../../../Images/" + res.AvatarPath))
                            {
                                message.Image = File.ReadAllBytes("../../../Images/" + res.AvatarPath);
                                message.FileNameImage = res.AvatarPath;
                            }
                            string json = JsonSerializer.Serialize(message);
                            sw.WriteLine(json);
                            sw.Flush();
                        }
                    }
                    else
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        sw.WriteLine(json);
                        sw.Flush();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(username))
                {
                    var res = db.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);
                    if (res != null)
                    {
                        if (res.Is2FAEnabled == true)
                        {
                            message.Account = res; // add email into account from db
                            var waiting_user = message;
                            waiting_user.Code2FA = new Random().Next(100000, 999999);
                            Waiting2FA.Add(waiting_user);
                            Task.Run(() => Send2FAMessage(waiting_user));
                            message.Message = "2FA";
                            string json = JsonSerializer.Serialize(message);
                            sw.WriteLine(json);
                            sw.Flush();
                        }
                        else
                        {
                            message.Message = "OK";
                            message.Account = res;
                            message.Autorization_Data = db.Autorization_Data.Where(a => a.AccountId == res.Id).ToList();
                            if (File.Exists("../../../Images/" + res.AvatarPath))
                            {
                                message.Image = File.ReadAllBytes("../../../Images/" + res.AvatarPath);
                                message.FileNameImage = res.AvatarPath;
                            }
                            string json = JsonSerializer.Serialize(message);
                            sw.WriteLine(json);
                            sw.Flush();
                        }
                    }
                    else
                    {
                        message.Message = "ERROR";
                        string json = JsonSerializer.Serialize(message);
                        sw.WriteLine(json);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message + "\nInner: " + ex.InnerException);
                Console.ResetColor();
                message.Message = "ERROR";
                string json = JsonSerializer.Serialize(message);
                sw.WriteLine(json);
                sw.Flush();
            }
            
        }

        private void Send2FAMessage(ServerMessage waiting_user)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.mailgun.org", 587);
            string[] lines = File.ReadAllLines("../../../email.txt");
            string login = lines[0];
            string password = lines[1];
            smtpClient.Credentials = new NetworkCredential(login, password);
            smtpClient.EnableSsl = true;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(login);
            mailMessage.To.Add(waiting_user.Account.Email);
            mailMessage.Subject = "2FA code";
            mailMessage.Body = $"Your 2FA code is: {waiting_user.Code2FA}";
            smtpClient.Send(mailMessage);
        }
    }
}
