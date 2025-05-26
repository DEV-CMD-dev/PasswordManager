using Client.Security;
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
    /// Interaction logic for SetNewPassword.xaml
    /// </summary>
    public partial class SetNewPassword : Window
    {
        public ServerMessage Message { get; set; }
        public IPEndPoint Server { get; set; }
        public SetNewPassword(ServerMessage message, IPEndPoint server)
        {
            InitializeComponent();
            Message = message;
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

        private void SaveNewPassword(object sender, RoutedEventArgs e)
        {
            var newPassword = tbPassword.Password;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please enter a new password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Message.Action = "ChangePassword";
            Message.Account.Password = PasswordHasher.HashPassword(newPassword);
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(Server);
                var ns = client.GetStream();
                StreamWriter sw = new StreamWriter(ns);
                string json = JsonSerializer.Serialize(Message);
                sw.WriteLine(json);
                sw.Flush();
                StreamReader sr = new StreamReader(ns);
                string response = sr.ReadLine();
                ServerMessage responseMessage = JsonSerializer.Deserialize<ServerMessage>(response);
                if (responseMessage.Message == "OK")
                {
                    MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Left = this.Left;
                    mainWindow.Top = this.Top;
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to change password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
