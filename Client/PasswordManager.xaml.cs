using Client.Security;
using PasswordManager.Database;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Timers;

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
            //AddPassword(); // for test
            var passwords = GetPasswords(); // for test

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
        public async void AddPassword()
        {
            string login = "dfgdfg@gmail.com"; // for test
            string password = "qwerty"; // for test
            string site = "https://www.google.com"; // for test
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

    }
}
