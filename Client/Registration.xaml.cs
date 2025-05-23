using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PasswordManager.Database;
using System.IO;
using System.Text.Json;
using System.Management;
using Client.Security;

namespace Client
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        const string IP = "127.0.0.1";
        const int PORT = 4444;
        TcpClient client;
        IPEndPoint server;
        public Registration()
        {
            InitializeComponent();
            server = new IPEndPoint(IPAddress.Parse(IP), PORT);
        }
        private void Login(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            mainWindow.Left = this.Left;
            mainWindow.Top = this.Top;

            mainWindow.Show();
            this.Close();
        }

        private string GetProcessorId()
        {
            var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? "Unknown";
            }
            return "Unknown";
        }

        private async void Register(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUsername.Text) || string.IsNullOrWhiteSpace(tbPassword.Password) || string.IsNullOrWhiteSpace(tbPasswordConfirm.Password))
            {
                return;
            }
            if (tbPassword.Password != tbPasswordConfirm.Password)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }
            try
            {
                Account account = new Account();

                account.Username = tbUsername.Text;
                account.Password = PasswordHasher.HashPassword(tbPassword.Password);
                account.Email = tbEmail.Text;

                client = new TcpClient();
                client.Connect(server);

                var ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);
                ServerMessage message = new ServerMessage();
                string json = message.RegisterJson(account);

                await sw.WriteLineAsync(json);
                await sw.FlushAsync();

                string responseJson = await sr.ReadLineAsync();
                message = JsonSerializer.Deserialize<ServerMessage>(responseJson);
                string response = message.Message;
                if (response == "OK")
                {
                    string token = GetProcessorId();
                    string tokenPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.txt");
                    File.WriteAllText(tokenPath, token);


                    MessageBox.Show("Registration successful");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Left = this.Left;
                    mainWindow.Top = this.Top;
                    mainWindow.Show();
                    this.Close();
                }
                else if (response == "ERROR")
                {
                    MessageBox.Show("Registration failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
