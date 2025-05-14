using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MainViewModel : INotifyPropertyChanged
{
    private string _autoSuggestBoxText;

    public string AutoSuggestBoxText
    {
        get => _autoSuggestBoxText;
        set
        {
            if (_autoSuggestBoxText != value)
            {
                _autoSuggestBoxText = value;
                OnPropertyChanged();
                UpdateSuggestions();
            }
        }
    }

    public ObservableCollection<string> AllOptions { get; } = new ObservableCollection<string>
    {
        "Password1", "Password2", "Password3", "Password4", "Password5"
    };

    public ObservableCollection<string> FilteredSuggestions { get; } = new ObservableCollection<string>();

    private void UpdateSuggestions()
    {
        FilteredSuggestions.Clear();

        foreach (var match in AllOptions
                     .Where(e => e.StartsWith(AutoSuggestBoxText ?? "", StringComparison.InvariantCultureIgnoreCase)))
        {
            FilteredSuggestions.Add(match);
        }
    }

    public MainViewModel()
    {
        UpdateSuggestions();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
