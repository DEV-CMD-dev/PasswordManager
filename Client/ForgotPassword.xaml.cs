using PasswordManager.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for ForgotPassword.xaml
    /// </summary>
    public partial class ForgotPassword : Window
    {
        public IPEndPoint Server { get; set; }
        public ForgotPassword(IPEndPoint server)
        {
            InitializeComponent();
            Server = server;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Left = this.Left;
            mainWindow.Top = this.Top;
            mainWindow.Show();
            this.Close();
        }

        private async void RecoverPasswordClick(object sender, RoutedEventArgs e)
        {
            var text = tbLogin.Text;
            ServerMessage message = new ServerMessage();
            Account account = new Account();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Please enter your login.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!text.Contains('@'))
            {
                account.Username = text;
            }
            else
            {
                account.Email = text;
            }
            message.Action = "RecoverPassword";
            message.Account = account;
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(Server);
                var ns = client.GetStream();
                StreamWriter sw = new StreamWriter(ns);
                string json = JsonSerializer.Serialize(message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
                StreamReader sr = new StreamReader(ns);
                string response = await sr.ReadLineAsync();
                ServerMessage responseMessage = JsonSerializer.Deserialize<ServerMessage>(response);
                if (responseMessage.Message == "OK")
                {
                    //MessageBox.Show("Recovery email sent successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    TwoStepVerification twoStepVerification = new TwoStepVerification(responseMessage, Server, File.ReadAllText("token.txt"), true); // !!!
                    twoStepVerification.Left = this.Left;
                    twoStepVerification.Top = this.Top;
                    twoStepVerification.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Password recovery failed. Please check your login and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Password recovery failed. Please check your login and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
