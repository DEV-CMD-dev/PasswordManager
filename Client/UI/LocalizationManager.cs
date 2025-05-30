using System.ComponentModel;
using System.Globalization;

namespace Client.UI
{
    

    public class LocalizationManager : INotifyPropertyChanged
    {
        private List<string> AvailableLanguages { get; } = new List<string>
        {
            "English",
            "Українська"
        };

        public List<string> GetAvailableLanguages()
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
        public string ProfileHintEmail => Resources.Strings.ProfileEmail;
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

        // Settings
        public string SettingsTitle => Resources.Strings.SettingsSafetyTextBox;
        public string Settings2FACheckbox => Resources.Strings.SettingsEnable2FACheckBox;
        public string SettingsAutoclearBufferCheckbox => Resources.Strings.SettingsAutoclearBufferCheckbox;
        public string SettingsEnableAutoLockAfter => Resources.Strings.SettingsEnableAutoLockAfter;
        public string SettingsAppearanceTextBlock => Resources.Strings.SettingsAppearanceTextBlock;
        public string SettingsThemeComboBox => Resources.Strings.SettingsThemeComboBox;
        public string SettingsLanguageComboBox => Resources.Strings.SettingsLanguageComboBox;

        // Password Generator
        public string GeneratePasswordTitle => Resources.Strings.GeneratePasswordTitle;
        public string GeneratePasswordLength => Resources.Strings.GeneratePasswordLength;
        public string GeneratePasswordIncludeUppercaseLetters => Resources.Strings.GeneratePasswordIncludeUppercaseLetters;
        public string GeneratePasswordIncludeLowercaseLetters => Resources.Strings.GeneratePasswordIncludeLowercaseLetters;
        public string GeneratePasswordIncludeNumbers => Resources.Strings.GeneratePasswordIncludeNumbers;
        public string GeneratePasswordIncludeSymbols => Resources.Strings.GeneratePasswordIncludeSymbols;


        public void ChangeLanguage(string cultureCode)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
