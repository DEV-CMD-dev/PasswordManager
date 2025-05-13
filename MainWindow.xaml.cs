using System.Text;
using System.Windows;
using System.Management;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace PasswordManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        string GetMotherboardSerialNumber()
        {
            var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString() ?? "Unknown";
            }
            return "Unknown";
        }

        private void SignUpClick(object sender, RoutedEventArgs e)
        {
            SignUP signUp = new SignUP();

            signUp.Left = this.Left;
            signUp.Top = this.Top;

            signUp.Show();
            this.Close();
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            bool success = true;
            if (success)
            {
                PasswordManagerWindow passwordManager = new PasswordManagerWindow();

                passwordManager.Show();
                this.Close();
            }
        }
    }
}