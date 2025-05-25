using System.ComponentModel;
using System.Runtime.CompilerServices;

public class PasswordItem : INotifyPropertyChanged
{
    public int Id { get; set; }
    private string _website;
    public string Website
    {
        get => _website;
        set { _website = value; OnPropertyChanged(); }
    }

    private string _user;
    public string User
    {
        get => _user;
        set { _user = value; OnPropertyChanged(); }
    }

    private string _password;
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }
    
    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set { _isFavorite = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
