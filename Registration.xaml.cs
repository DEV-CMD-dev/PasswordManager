using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PasswordManager
{
    /// <summary>
    /// Interaction logic for SignUP.xaml
    /// </summary>
    public partial class SignUP : Window
    {
        public SignUP()
        {
            InitializeComponent();
        }

        private void OpenLoginWindow()
        {
            MainWindow mainWindow = new MainWindow();

            mainWindow.Left = this.Left;
            mainWindow.Top = this.Top;

            mainWindow.Show();
            this.Close();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            OpenLoginWindow();
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            bool succes = true;
            if (succes)
            {
                OpenLoginWindow();
            }
        }
    }
}
