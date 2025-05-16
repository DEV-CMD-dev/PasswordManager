using Client.Security;
using PasswordManager.Database;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;

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
        }
        public List<Autorization_data> GetPasswords()
        {
            var passwordHasher = new PasswordHasher(DescryptionToken);
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

        public async void UpdateProfile(ServerMessage message)
        {
            var account = message.Account;
            if (!string.IsNullOrWhiteSpace(account.Username))
            {
                Username.Text = account.Username;
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
            var passwordHasher = new PasswordHasher(DescryptionToken);
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
    }
}
