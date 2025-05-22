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
    /// Interaction logic for TwoStepVerification.xaml
    /// </summary>
    public partial class TwoStepVerification : Window 
    {
        public ServerMessage Message { get; set; }
        public IPEndPoint Server { get; set; }
        public string ProcessorId { get; set; }
        public int Counter { get; set; } = 0;
        public TwoStepVerification(ServerMessage message, IPEndPoint server, string processorId)
        {
            InitializeComponent();
            Message = message;
            Server = server;
            ProcessorId = processorId;
        }

        private async void SendAgain(object sender, RoutedEventArgs e)
        {
            if (Counter >= 3)
            {
                MessageBox.Show("You have reached the maximum number of attempts(3) to send the code again.");
                return;
            }
            Message.Action = "2FASendAgain";
            TcpClient client = new TcpClient();
            client.Connect(Server);
            var ns = client.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            string json = JsonSerializer.Serialize(Message);
            await sw.WriteLineAsync(json);
            await sw.FlushAsync();
            string responseJson = await sr.ReadLineAsync();
            Message = JsonSerializer.Deserialize<ServerMessage>(responseJson);
            string response = Message.Message;
            if (response == "OK")
            {
                MessageBox.Show("Code sent again");
                Counter++;
            }
            else
            {
                MessageBox.Show("Error sending code");
            }

        }

        private async void Confirm2FACode(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbCode.Text.Length < 6 || tbCode.Text.Length > 6)
                {
                    MessageBox.Show("Must be a 6 digits!");
                    return;
                }
                Message.Action = "2FAPassword";
                Message.Code2FA = int.Parse(tbCode.Text);
                TcpClient client = new TcpClient();
                client.Connect(Server);
                var ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);
                string json = JsonSerializer.Serialize(Message);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
                string responseJson = await sr.ReadLineAsync();
                Message = JsonSerializer.Deserialize<ServerMessage>(responseJson);
                string response = Message.Message;
                if (response == "OK")
                {
                    PasswordManagerWindow window = new PasswordManagerWindow(Message, ProcessorId, Server);
                    window.Left = this.Left;
                    window.Top = this.Top;
                    window.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect code");
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error: " + ex.Message);
                return;
            }
            
        }
    }
}
