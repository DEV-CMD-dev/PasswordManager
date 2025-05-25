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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;


namespace Client
{
    /// <summary>
    /// Interaction logic for _1.xaml
    /// </summary>
    public partial class PasswordManagerWindow : Window
    {
        public IEnumerable<Swatch> ColorList { get; set; } = ThemeHelper.GetAvaliableColors();
        public ObservableCollection<PasswordItem> Passwords { get; set; }
        public LocalizationManager Localization => LocalizationManager.Instance;

        public ServerMessage Message { get; set; }
        public string DescryptionToken { get; set; }
        public IPEndPoint Server { get; set; }
        public PasswordManagerWindow(ServerMessage message, string descryptionToken, IPEndPoint server)
        {
            InitializeComponent();
            //DataContext = new MainViewModel();
            Message = message;
            PasswordList.ItemsSource = Passwords;
            DescryptionToken = descryptionToken;
            Server = server;
            if (message.Account.Is2FAEnabled)
            {
                cb2FA.IsChecked = true;
            }
            Passwords = GetPasswords();
            UpdateProfile();
            //AddImage("../../../account(1).png", message); // test
            // ChangePassword(message, "test"); // test
            //RemovePassword(Message.Autorization_Data[0]); // works, test
            InitializeInactivityTimer();
            HookUserActivity();
            
            
            DataContext = this;
        }
        private void RemovePassword(Autorization_data password)
        {
            if (password is null)
            {
                return;
            }
            Message.Action = "RemovePassword";
            TcpClient client = new TcpClient();
            client.Connect(Server);
            var ns = client.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            StreamReader sr = new StreamReader(ns);
            Message.Autorization_Data = new List<Autorization_data> { password };
            var json = JsonSerializer.Serialize(Message);
            sw.WriteLine(json);
            sw.Flush();
            var responseJson = sr.ReadLine();
            var responseMessage = JsonSerializer.Deserialize<ServerMessage>(responseJson);
            if (responseMessage != null && responseMessage.Message == "OK")
            {
                MessageBox.Show("Password removed successfully.");
                GetPasswords();
            }
            else
            {
                MessageBox.Show("Error removing password.");
            }
        }
        
        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            var availableLanguages = Localization.GetAvailableLanguages();
            var item = LanguageSettingsCb.SelectedItem as ComboBoxItem;
            var selectedLanguage = item.Content as string;

            if (!availableLanguages.Contains(selectedLanguage))
                return;

            switch(selectedLanguage)
            {
                case "English":
                    Localization.ChangeLanguage("");
                    break;
                case "Українська":
                    Localization.ChangeLanguage("uk-UA");
                    break;
                default:
                    Localization.ChangeLanguage("");
                    break;
            }
        }

        private void UpdatePasswords()
        {
            foreach (var password in Passwords)
            {
                PasswordList.Items.Add(password);
            }
            UpdatePasswordBoxes();
        }

        private async void UpdatePasswordBoxes()
        {
            await Task.Delay(2000);

            for (int i = 0; i < PasswordList.Items.Count; i++)
            {
                var container = (ContentPresenter)PasswordList.ItemContainerGenerator.ContainerFromIndex(i);
                if (container == null)
                {
                    if (PasswordList.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        await Task.Delay(500);
                        container = (ContentPresenter)PasswordList.ItemContainerGenerator.ContainerFromIndex(i);
                    }
                    if (container == null)
                        continue;
                }

                var passwordBox = FindVisualChild<PasswordBox>(container);
                if (passwordBox != null)
                {
                    var item = PasswordList.Items[i] as PasswordItem;
                    if (item != null)
                        passwordBox.Password = item.Password;
                }
            }
        }


        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
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

        public async Task<ObservableCollection<PasswordItem>> GetPasswordsAsync()
        {
            return Task.Run(() => GetPasswords()).Result;
        }

        public ObservableCollection<PasswordItem> GetPasswords()
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
                return new ObservableCollection<PasswordItem>();
            }
            Message = responseMessage;
            // decrypt passwords
            var passwordHasher = new PasswordCryptor(DescryptionToken);
            var passwordsEncrypted = Message.Autorization_Data;
            if (passwordsEncrypted is null) // not have a passwords
            {
                return new ObservableCollection<PasswordItem>();
            }
            var passwordsDecrypted = new ObservableCollection<PasswordItem>();
            foreach (var password in passwordsEncrypted)
            {
                var decryptedPassword = new PasswordItem
                {
                    User = passwordHasher.DecryptPassword(password.Login),
                    Password = passwordHasher.DecryptPassword(password.Password),
                    Website = password.Site,
                    IsFavorite = password.IsFavourite
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
            if (Message.Image != null)
            {
                File.WriteAllBytes("../../../"+Message.FileNameImage, Message.Image);
                if (File.Exists("../../../"+Message.FileNameImage))
                {
                    AvatarImageBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath("../../../"+Message.FileNameImage))); // main pos
                    HeaderAvatarBrush.ImageSource = new BitmapImage(new Uri(Path.GetFullPath("../../../" + Message.FileNameImage))); // left panel pos
                }
            }
            UpdatePasswords();
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

        private void DeletePassword(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is object item)
            {
                if (PasswordList.ItemsSource is System.Collections.IList list)
                {
                    list.Remove(item);
                }
                else if (PasswordList.Items.Contains(item))
                {
                    PasswordList.Items.Remove(item);
                    //RemovePassword()
                    //PasswordCryptor passwordCryptor = new PasswordCryptor(DescryptionToken);
                    //PasswordItem passwordEncrypted = new PasswordItem()
                    //{
                    //    Website = (((PasswordItem)item).Website),
                    //    User = passwordCryptor.EncryptPassword(((PasswordItem)item).User),
                    //    Password = passwordCryptor.EncryptPassword(((PasswordItem)item).Password)
                    //};
                    //foreach (var password in Message.Autorization_Data)
                    //{
                    //    if (password.Site == passwordEncrypted.Website &&
                    //        password.Login == passwordEncrypted.User && // another login & password, error
                    //        password.Password == passwordEncrypted.Password)
                    //    {
                    //        RemovePassword(password);
                    //        break;
                    //    }
                    //}
                    //not work
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
        public async void GeneratePassword(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isLowercase = IncludeLowercaseLetters.IsChecked ?? false;
                bool isUppercase = IncludeUppercaseLetters.IsChecked ?? false;
                bool isNumbers = IncludeNumbers.IsChecked ?? false;
                bool isSymbols = IncludeSymbols.IsChecked ?? false;

                if(int.TryParse(GeneratePasswordLength.Text.ToString(), out int length) == false || length < 8 || length > 100)
                {
                    return;
                }

                string password = await Task.Run(() => PasswordGenerator.Generate(length, isLowercase,isUppercase,isNumbers,isSymbols));
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

        // copy pass
        private void CopyGeneratedPasswordButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedPasswordTextBox.Text))
            {
                Clipboard.SetText(GeneratedPasswordTextBox.Text);
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
                        tbCurrentPassword.Clear();
                        tbNewPassword.Clear();
                        tbConfirmPassword.Clear();
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

        void ChangePasswordBtn(object sender, RoutedEventArgs e)
        {
            // add checking strength password
            string current = tbCurrentPassword.Text;
            string newPass = tbNewPassword.Text;
            string confirm = tbConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirm))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            if (current == newPass)
            {
                ShowError("New password must be different from current.");
                return;
            }

            if (PasswordHasher.HashPassword(current) != Message.Account.Password)
            {
                ShowError("Current password is incorrect.");
                return;
            }

            if (newPass != confirm)
            {
                ShowError("Passwords do not match.");
                return;
            }

            if (newPass.Length < 8 || newPass.Length > 40)
            {
                ShowError("Password must be between 8 and 40 characters long.");
                return;
            }

            ChangePassword(Message, tbNewPassword.Text);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
