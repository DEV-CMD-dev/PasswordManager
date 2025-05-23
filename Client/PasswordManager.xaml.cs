using Client.Security;
using PasswordManager.Database;
using System.IO;
using Microsoft.Win32;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Media;
using Client.UI;
using MaterialDesignColors;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Documents;


namespace Client
{
    /// <summary>
    /// Interaction logic for _1.xaml
    /// </summary>
    public partial class PasswordManagerWindow : Window
    {
        public IEnumerable<Swatch> ColorList { get; set; } = ThemeHelper.GetAvaliableColors();

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
            if (message.Account.Is2FAEnabled)
            {
                cb2FA.IsChecked = true;
            }
            //AddPassword("test", "test", "https://google.com"); // for test
            var passwords = GetPasswords(); // for test
            UpdateProfile();
            //AddImage("../../../account(1).png", message); // test
            // ChangePassword(message, "test"); // test

            InitializeInactivityTimer();
            HookUserActivity();

            DataContext = this;
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

        public async Task<List<Autorization_data>> GetPasswordsAsync()
        {
            return Task.Run(() => GetPasswords()).Result;
        }

        public List<Autorization_data> GetPasswords()
        {
            // get passwords from db
            Message.Action = "GetPasswords";
            var json = JsonSerializer.Serialize(Message);
            TcpClient client = new TcpClient();
            client.Connect(Server);
            var ns = client.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            StreamReader sr = new StreamReader(ns);
            sw.WriteLine(json);
            sw.Flush();
            var responseJson = sr.ReadLine();
            var responseMessage = JsonSerializer.Deserialize<ServerMessage>(responseJson);
            if (responseMessage == null || responseMessage.Message != "OK")
            {
                MessageBox.Show("Error getting passwords.");
                return new List<Autorization_data>();
            }
            Message = responseMessage;
            // decrypt passwords
            var passwordHasher = new PasswordCryptor(DescryptionToken);
            var passwordsEncrypted = Message.Autorization_Data;
            if (passwordsEncrypted is null) // not have a passwords
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

        public void UpdateProfile()
        {
            var account = Message.Account;
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
                    message = responseMessage;
                    message.Message = "";
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

        // click avatar func on left button
        private void AvatarBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                if (Message?.Account == null)
                {
                    MessageBox.Show("Could not to find account details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    AddImage(imagePath, Message);
                    AvatarImageBrush.ImageSource = new BitmapImage(new Uri(imagePath)); // main pos
                    HeaderAvatarBrush.ImageSource = new BitmapImage(new Uri(imagePath)); // left panel pos
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not to upload a image" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // generating password
        private async void PasswordLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int length = (int)e.NewValue;
            try
            {
                string password = await Task.Run(() => PasswordGenerator.Generate(length));
                GeneratedPasswordTextBox.Text = password;

                var strength = await Task.Run(() => PasswordStrengthEvaluator.Evaluate(password));
                PasswordStrengthTextBlock.Text = PasswordStrengthEvaluator.GetStrengthText(strength);

                PasswordStrengthStarsTextBlock.Inlines.Clear();

                int stars = (int)strength;
                int maxStars = 5;

                for (int i = 0; i < stars; i++)
                {
                    var star = new Run("★")
                    {
                        Foreground = new SolidColorBrush(Colors.Green)
                    };
                    PasswordStrengthStarsTextBlock.Inlines.Add(star);
                }
                for (int i = stars; i < maxStars; i++)
                {
                    var star = new Run("★")
                    {
                        Foreground = new SolidColorBrush(Colors.LightGray)
                    };
                    PasswordStrengthStarsTextBlock.Inlines.Add(star);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating password: {ex.Message}");
            }
        }

        //public async void GeneratePasswordButtonClick(object sender, RoutedEventArgs e)
        //{
        //    int length = (int)PasswordLengthSlider.Value;
        //    try
        //    {
        //        string password = await Task.Run(() => PasswordGenerator.Generate(length));
        //        GeneratedPasswordTextBox.Text = password;

        //        var strength = await Task.Run(() => PasswordStrengthEvaluator.Evaluate(password));
        //        PasswordStrengthTextBlock.Text = PasswordStrengthEvaluator.GetStrengthText(strength);

        //        PasswordStrengthStarsTextBlock.Inlines.Clear();

        //        int stars = (int)strength;
        //        int maxStars = 5;

        //        for (int i = 0; i < stars; i++)
        //        {
        //            var star = new Run("★")
        //            {
        //                Foreground = new SolidColorBrush(Colors.Green)
        //            };
        //            PasswordStrengthStarsTextBlock.Inlines.Add(star);
        //        }
        //        for (int i = stars; i < maxStars; i++)
        //        {
        //            var star = new Run("★")
        //            {
        //                Foreground = new SolidColorBrush(Colors.LightGray)
        //            };
        //            PasswordStrengthStarsTextBlock.Inlines.Add(star);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error generating password: {ex.Message}");
        //    }
        //}
        // copy pass
        private void CopyPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedPasswordTextBox.Text))
            {
                Clipboard.SetText(GeneratedPasswordTextBox.Text);
                MessageBox.Show("Password copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No password to copy.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async void ChangePassword(ServerMessage message, string newPassword)
        {
            try
            {
                message.Action = "ChangePassword";
                message.Account.Password = PasswordHasher.HashPassword(newPassword);
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
                if (responseMessage != null)
                {
                    if (responseMessage.Message == "OK")
                    {
                        MessageBox.Show("Password changed successfully.");
                        message = responseMessage;
                        message.Message = "";
                    }
                    else
                    {
                        MessageBox.Show("Error changing password.");
                    }
                }
                else
                {
                    MessageBox.Show("Error changing password.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error changing password.");
            }
            
        }

        private void ChangePasswordBtn(object sender, RoutedEventArgs e)
        {
            // add checking strength password
            if (string.IsNullOrWhiteSpace(tbCurrentPassword.Text) || string.IsNullOrWhiteSpace(tbNewPassword.Text) || string.IsNullOrEmpty(tbConfirmPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }
            if (tbCurrentPassword.Text == tbNewPassword.Text)
            {
                MessageBox.Show("Password must be another", "Error", MessageBoxButton.OK, MessageBoxImage.Error); // change to another
                return;
            }
            if (tbCurrentPassword.Text != Message.Account.Password)
            {
                MessageBox.Show("Current password is incorrect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (tbNewPassword.Text != tbConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (tbNewPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (tbNewPassword.Text.Length > 40)
            {
                MessageBox.Show("Password must be no more than 40 characters long", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ChangePassword(Message, tbNewPassword.Text);
        }

        private async void Update2FAStatus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cb2FA.IsChecked == true)
                {
                    Message.Account.Is2FAEnabled = true;
                }
                else
                {
                    Message.Account.Is2FAEnabled = false;
                }
                Message.Action = "UpdateProfile";
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
                if (responseMessage != null)
                {
                    if (responseMessage.Message == "OK")
                    {
                        Message = responseMessage;
                        Message.Message = "";
                    }
                    else
                    {
                        MessageBox.Show("Error change option 2FA.");
                    }
                }
                else
                {
                    MessageBox.Show("Error change option 2FA.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error change option 2FA.");
            }
        }
    }
}
