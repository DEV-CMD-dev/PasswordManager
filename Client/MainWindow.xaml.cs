﻿using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using PasswordManager.Database;
using System.Text.Json;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using Client.Security;

namespace Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    const string IP = "127.0.0.1";
    const int PORT = 4444;
    TcpClient client;
    IPEndPoint server;
    public MainWindow()
    {
        InitializeComponent();
        server = new IPEndPoint(IPAddress.Parse(IP), PORT);

        string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.txt");

        if (!File.Exists(tokenPath))
        {
            MessageBox.Show("Token not found. Please register first.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);

            tbLogin.IsEnabled = false;
            tbPassword.IsEnabled = false;
            btnLogin.IsEnabled = false;
        }

    }


    private string GetProcessorId() // after delete
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
            account.Password = PasswordHasher.HashPassword(tbPassword.Password);
            client = new TcpClient();

            await client.ConnectAsync(server);
            var ns = client.GetStream();

            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            ServerMessage message = new ServerMessage();

            string json = message.LoginJson(account);

            await sw.WriteLineAsync(json);
            await sw.FlushAsync();

            while (true)
            {
                string responseJson = await sr.ReadLineAsync();
                message = JsonSerializer.Deserialize<ServerMessage>(responseJson);
                string response = message.Message;

                string tokenPath = "token.txt";
                string token = "";

                if (File.Exists(tokenPath))
                {
                    token = File.ReadAllText(tokenPath);
                }
                else
                {
                    token = "Unknown";
                    MessageBox.Show("No token", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (response == "OK")
                {
                    PasswordManagerWindow passwordManager = new PasswordManagerWindow(message, token, server); // token from file
                    passwordManager.Show();

                    //passwordManager.UpdateProfile(message);

                    this.Close();
                    break;
                }
                else if (response == "2FA")
                {
                    // new window
                    TwoStepVerification window = new TwoStepVerification(message, server, token);
                    window.Left = this.Left;
                    window.Top = this.Top;
                    window.Show();
                    this.Close();
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

    private void ForgotPasswordClick(object sender, RoutedEventArgs e)
    {
        ForgotPassword forgotPassword = new ForgotPassword(server);
        forgotPassword.Left = this.Left;
        forgotPassword.Top = this.Top;
        forgotPassword.Show();
        this.Close();
    }
}