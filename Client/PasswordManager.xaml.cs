using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for _1.xaml
    /// </summary>
    public partial class PasswordManagerWindow : Window
    {
        public PasswordManagerWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
