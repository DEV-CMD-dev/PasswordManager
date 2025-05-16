using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using PasswordManager.Database;
using System.Text.Json;

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const string IP = "127.0.0.1";
    const int PORT = 4444;
    TcpClient client;
    //HttpClient client;
    IPEndPoint server;
    public MainWindow()
    {
        InitializeComponent();
        server = new IPEndPoint(IPAddress.Parse(IP), PORT);
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


    private void SignUpClick(object sender, RoutedEventArgs e)
    {
        Registration signUp = new Registration();

        signUp.Left = this.Left;
        signUp.Top = this.Top;

        signUp.Show();
        this.Close();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbLogin.Text) || string.IsNullOrWhiteSpace(tbPassword.Password))
        {
            
            return;
        }
        try
        {
            Account account = new Account();
            if (tbLogin.Text.Contains('@'))
            {
                account.Email = tbLogin.Text;
            }
            else
            {
                account.Username = tbLogin.Text;
            }
            account.Password = tbPassword.Password;
            client = new TcpClient();

            await client.ConnectAsync(server);
            var ns = client.GetStream();

            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            string json = JsonSerializer.Serialize(account);

            await sw.WriteLineAsync(json);
            await sw.FlushAsync();

            while (true)
            {
                string response = await sr.ReadLineAsync();
                if (response == "OK")
                {
                    PasswordManagerWindow passwordManager = new PasswordManagerWindow();
                    passwordManager.Show();

                    this.Close();
                    break;
                }
                else if (response == "2FA")
                {
                    // new window
                    break;
                }
                else if (response == "ERROR")
                {
                    MessageBox.Show("Login failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}