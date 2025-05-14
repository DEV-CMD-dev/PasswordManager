using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            listener.Start();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server started!");
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                var ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);
                string json = await sr.ReadToEndAsync();
            }
        }

    }
}
