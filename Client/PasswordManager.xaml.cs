using Client.Security;
using PasswordManager.Database;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Media;

namespace Client
{
    /// <summary>
    /// Interaction logic for _1.xaml
    /// </summary>
    public partial class PasswordManagerWindow : Window
    {
        public ServerMessage Message { get; set; }
        public string DescryptionToken { get; set; }
        public IPEndPoint Server { get; set; }
        public PasswordManagerWindow(ServerMessage message, string descryptionToken, IPEndPoint server)
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Message = message;
            DescryptionToken = descryptionToken;
            Server = server;
            //AddPassword("test", "test", "https://google.com"); // for test
            var passwords = GetPasswords(); // for test
            //AddImage("../../../account(1).png", message); // test

            InitializeInactivityTimer();
            HookUserActivity();
        }

        // timer
        private System.Timers.Timer inactivityTimer;
        private readonly TimeSpan timeout = TimeSpan.FromMinutes(2);
        //private readonly TimeSpan timeout = TimeSpan.FromSeconds(5);

        private void InitializeInactivityTimer()
        {
            inactivityTimer = new System.Timers.Timer(timeout.TotalMilliseconds);
            inactivityTimer.Elapsed += OnInactivityTimeout;
            inactivityTimer.AutoReset = false;
            inactivityTimer.Start();
        }

        private void HookUserActivity()
        {
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;
        }

        private void ResetInactivityTimer(object sender, EventArgs e)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        private void OnInactivityTimeout(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = new MainWindow();

                mainWindow.Left = this.Left;
                mainWindow.Top = this.Top;

                this.Close();
                mainWindow.Show();
                
            });
        }

        public List<Autorization_data> GetPasswords()
        {
            var passwordHasher = new PasswordCryptor(DescryptionToken);
            var passwordsEncrypted = Message.Autorization_Data;
            if (passwordsEncrypted is null) // not have a password
            {
                return new List<Autorization_data>();
            }
            var passwordsDecrypted = new List<Autorization_data>();
            foreach (var password in passwordsEncrypted)
            {
                var decryptedPassword = new Autorization_data
                {
                    Login = passwordHasher.DecryptPassword(password.Login),
                    Password = passwordHasher.DecryptPassword(password.Password),
                    Site = password.Site,
                };
                passwordsDecrypted.Add(decryptedPassword);
            }
            return passwordsDecrypted;
        }

        public void UpdateProfile(ServerMessage message)
        {
            var account = message.Account;
            if (!string.IsNullOrWhiteSpace(account.Username))
            {
                UsernameData.Text = Username.Text = account.Username;
            }
        }

        public async void AddPassword(string login, string password, string site = "")
        {
            /*string login = tbLogin.Text;
            string password = tbPassword.Text;
            string site = tbSite.Text;*/
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(site))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }
            var passwordHasher = new PasswordCryptor(DescryptionToken);
            var encryptedPassword = new Autorization_data
            {
                Login = passwordHasher.EncryptPassword(login),
                Password = passwordHasher.EncryptPassword(password),
                Site = site,
                AccountId = Message.Account.Id
            };

            Message.NewPassword = encryptedPassword;
            Message.Action = "AddPassword";
            Message.Message = "";

            var json = JsonSerializer.Serialize(Message);
            TcpClient client = new TcpClient();
            client.Connect(Server);
            var ns = client.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            StreamReader sr = new StreamReader(ns);

            await sw.WriteLineAsync(json);
            await sw.FlushAsync();

            var responseJson = await sr.ReadLineAsync();
            var responseMessage = JsonSerializer.Deserialize<ServerMessage>(responseJson);
            if (responseMessage.Message == "OK")
            {
                MessageBox.Show("Password added successfully.");
                GetPasswords();
            }
            else
            {
                MessageBox.Show("Error adding password.");
            }
        }

        private void AddPasswordLine(object sender, RoutedEventArgs e)
        {
            PasswordList.Items.Add("");
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var grid = VisualTreeHelper.GetParent(button);
            while (grid != null && !(grid is Grid))
            {
                grid = VisualTreeHelper.GetParent(grid);
            }

            if (grid is Grid parentGrid)
            {
                foreach (var child in parentGrid.Children)
                {
                    if (child is PasswordBox passwordBox)
                    {
                        Clipboard.SetText(passwordBox.Password);
                        break;
                    }
                    else if(child is TextBox textBox)
                    {
                        Clipboard.SetText(textBox.Text);
                        break;
                    }
                }
            }
        }

        //need to fix
        private void DeletePassword(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext != null)
            {
                var item = button.DataContext;

                if (PasswordList.ItemsSource is IList<object> sourceList)
                {
                    int index = sourceList.IndexOf(item);
                    if (index >= 0)
                    {
                        sourceList.RemoveAt(index);
                    }
                }
                else if (PasswordList.Items.Contains(item))
                {
                    PasswordList.Items.Remove(item);
                }
                else
                {
                    MessageBox.Show("Error deleting password.");
                }
            }
        }

        public async void AddImage(string imagepath, ServerMessage message)
        {
            if (Path.Exists(imagepath))
            {
                message.Image = File.ReadAllBytes(imagepath);
                message.FileNameImage = message.Account.Id + Path.GetExtension(imagepath);
                message.Action = "AddImage";
                message.Message = "";
                var json = JsonSerializer.Serialize(message);
                TcpClient client = new TcpClient();
                client.Connect(Server);
                var ns = client.GetStream();
                StreamWriter sw = new StreamWriter(ns);
                StreamReader sr = new StreamReader(ns);
                await sw.WriteLineAsync(json);
                await sw.FlushAsync();
                var responseJson = await sr.ReadLineAsync();
                var responseMessage = JsonSerializer.Deserialize<ServerMessage>(responseJson);
                if (responseMessage.Message == "OK")
                {
                    MessageBox.Show("Image added successfully.");
                }
                else
                {
                    MessageBox.Show("Error adding image.");
                }
            }
            else
            {
                MessageBox.Show("Image not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }




    }
}
