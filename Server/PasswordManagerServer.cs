using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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
        public PasswordManagerServer()
        {
            server = new IPEndPoint(IPAddress.Parse(IP), PORT);
            listener = new TcpListener(server);
            db = new PasswordManager_db();
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

                    if (message != null)
                    {
                        if (message.Action == "Login")
                        {
                            Login(message, sw);
                        }
                        else if (message.Action == "Register")
                        {
                            Register(message, sw);
                        }
                        else if (message.Action == "AddPassword")
                        {
                            AddPassword(message, sw);
                        }
                        /*else if (message.Action == "UpdateProfile")
                        {
                            UpdateProfile(message, sw);
                        }
                        else if (message.Action == "GetPasswords")
                        {
                            GetPasswords(message, sw);
                        }
                        else if (message.Action == "UpdatePassword")
                        {
                            UpdatePassword(message, sw);
                        }*/
                        else if (message.Action == "AddImage")
                        {
                            AddImage(message, sw);
                        }
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
                            message.Code2FA = new Random().Next(100000, 999999); // send to email, if 2FA enabled
                            message.Account = res;
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
                            message.Code2FA = new Random().Next(100000, 999999);
                            message.Account = res;
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

    }
}
