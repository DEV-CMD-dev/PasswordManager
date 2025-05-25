using System.ComponentModel;
using System.Globalization;

namespace Client.UI
{
    

    public class LocalizationManager : INotifyPropertyChanged
    {
        private IEnumerable<string> AvailableLanguages { get; } = new List<string>
        {
            "English",
            "Українська"
        };

        public IEnumerable<string> GetAvailableLanguages()
        {
            return AvailableLanguages;
        }

        public static LocalizationManager Instance { get; } = new();

        // Tab controls
        public string TabProfile => Resources.Strings.TabControlProfile;
        public string TabPasswords => Resources.Strings.TabControlPasswords;
        public string TabFavorites => Resources.Strings.TabControlFavorites;
        public string TabSettings => Resources.Strings.TabControlSettings;
        public string TabPasswordGenerator => Resources.Strings.TabControlPasswordGenerator;



        // Profile
        public string ProfileHintUsername => Resources.Strings.ProfileUsernameTextBoxHint;
        public string ProfileTitle => Resources.Strings.ProfileTitle;
        public string ProfilePersonalInfo => Resources.Strings.ProfilePersonalInformationTextBox;
        public string ProfileChangePassword => Resources.Strings.ProfileChangePasswordTextBox;
        public string ProfileHintName => Resources.Strings.ProfileNameTextBoxHint;
        public string ProfileHintSurname => Resources.Strings.ProfileSurnameTextBoxHint;
        public string ProfileHintCurrentPassword => Resources.Strings.ProfileCurrentPasswordTextBoxHint;
        public string ProfileHintNewPassword => Resources.Strings.ProfileNewPasswordTextBoxHint;
        public string ProfileHintConfirmPassword => Resources.Strings.ProfileConfirmPasswordTextBoxHint;
        public string ProfileApplyChanges => Resources.Strings.ProfileApplyChangesButton;

        // Passwords
        public string PasswordsTitle => Resources.Strings.PasswordsTitle;
        public string PasswordsWebsiteHint => Resources.Strings.PasswordsWebsiteTextBoxHint;
        public string PasswordsUserHint => Resources.Strings.PasswordsUserTextBoxHint;
        public string PasswordsPasswordHint => Resources.Strings.PasswordsPasswordTextBoxHint;
        public string PasswordsDeleteButton => Resources.Strings.PasswordsDeleteButton;


        public void ChangeLanguage(string cultureCode)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
