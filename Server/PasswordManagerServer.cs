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
                    Account account = JsonSerializer.Deserialize<Account>(json);
                    if (account != null)
                    {
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
                                    sw.WriteLine("2FA");
                                    sw.Flush();
                                }
                                else
                                {
                                    sw.WriteLine("OK");
                                    sw.Flush();
                                }
                            }
                            else
                            {
                                sw.WriteLine("ERROR");
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
                                    sw.WriteLine("2FA");
                                    sw.Flush();
                                }
                                else
                                {
                                    sw.WriteLine("OK");
                                    sw.Flush();
                                }
                            }
                            else
                            {
                                sw.WriteLine("ERROR");
                                sw.Flush();
                            }
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

    }
}
