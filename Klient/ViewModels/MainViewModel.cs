using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Klient.Core.Api;
using Klient.Core.Model;

namespace Klient.ViewModels;

public class MainViewModel : ObservableRecipient {

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    //login section
    private string _login = string.Empty;
    public string Login {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _password = string.Empty;
    public string Password {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    //register section
    private string _register_name = string.Empty;
    public string RegisterName {
        get => _register_name;
        set => SetProperty(ref _register_name, value);
    }

    private string _register_surname = string.Empty;
    public string RegisterSurname {
        get => _register_surname;
        set => SetProperty(ref _register_surname, value);
    }

    private string _register_login = string.Empty;
    public string RegisterLogin {
        get => _register_login;
        set => SetProperty(ref _register_login, value);
    }

    private string _register_password = string.Empty;
    public string RegisterPassword {
        get => _register_password;
        set => SetProperty(ref _register_password, value);
    }

    private string _register_password_confirm = string.Empty;
    public string RegisterPasswordConfirm {
        get => _register_password_confirm;
        set => SetProperty(ref _register_password_confirm, value);
    }

    // token
    private string _token = string.Empty;
    public string Token {
        get => _token;
        set => SetProperty(ref _token, value);
    }

    public DateTime _tokenExpireDate = DateTime.MinValue;
    public DateTime TokenExpireDate {
        get => _tokenExpireDate;
        set => SetProperty(ref _tokenExpireDate, value);
    }

    private string _tokenExpireDateString = "";
    public string TokenExpireDateString {
        get => _tokenExpireDateString;
        set => SetProperty(ref _tokenExpireDateString, value);
    }

    //methods

    public MainViewModel() {
        try {
            using BinaryReader reader = new(new FileStream("token.bin", FileMode.Open, FileAccess.ReadWrite));
            Token = reader.ReadString();
        } catch (Exception) { }
    }

    private static async Task<ContentDialogResult> DisplayError(object sender, string errorMessage, string errorTitle = "Błąd", string errorButtonText = "OK") {
        ContentDialog dialog = new() {
            XamlRoot = ((Button)sender).XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = errorTitle,
            Content = errorMessage,
            PrimaryButtonText = errorButtonText,
            DefaultButton = ContentDialogButton.Primary
        };

        return await dialog.ShowAsync();
    }

    internal async void RegisterButtonClick(object sender, RoutedEventArgs e, MainViewModel vm) {
        string errorMessage = string.Empty;
        bool displayError = false;

        if (_login.Length < 3 || _login.Length > 100) {
            errorMessage = "Login musi zawierać od 3 do 100 znaków";
            displayError = true;
        } else if (_password.Length < 3 || _password.Length > 100) {
            errorMessage = "Hasło musi zawierać od 3 do 100 znaków";
            displayError = true;
        }
        if (displayError) {
            await DisplayError(sender, errorMessage);
            return;
        }
        IsLoading = true;

        vm.RegisterLogin = "";
        vm.RegisterName = "";
        vm.RegisterSurname = "";
        vm.RegisterPassword = "";
        vm.RegisterPasswordConfirm = "";

        IsLoading = false;
    }

    internal async void LoginButtonClick(object sender, RoutedEventArgs e, MainViewModel vm) {
        string errorMessage = string.Empty;
        bool displayError = false;

        if (_login.Length < 3 || _login.Length > 100) {
            errorMessage = "Login musi zawierać od 3 do 100 znaków";
            displayError = true;
        } else if (_password.Length < 3 || _password.Length > 100) {
            errorMessage = "Hasło musi zawierać od 3 do 100 znaków";
            displayError = true;
        }

        if (displayError) {
            await DisplayError(sender, errorMessage);
            return;
        }

        IsLoading = true;
        Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
        IUserApi userApi = new UserApi();
        UserLoginDTO userLoginDTO = new() { Username = _login, Password = _password };
        try {
            string _token_response = await userApi.ApiUserLoginPostAsync(userLoginDTO);
            if (_token_response != null) {
                Token = _token_response.Replace("\"", string.Empty);
                TokenExpireDateString = $"Twój token wygaśnie {DateTime.Now.AddDays(1):dd.MM.yyyy HH:mm:ss}.";
                vm.Login = "";
                vm.Password = "";

                //save token to binary file token.bin using binarystream
                using BinaryWriter writer = new(new FileStream("token.bin", FileMode.Create, FileAccess.ReadWrite));
                writer.Write(Token);
                IsLoading = false;
            }
        } catch (Exception) {
            await DisplayError(sender, "Niepoprawne dane");
        }
        IsLoading = false;
    }

    internal static void LogoutButtonClick(object sender, RoutedEventArgs e, MainViewModel vm) {
        vm.Token = string.Empty;
        vm.TokenExpireDate = DateTime.MinValue;
        vm.TokenExpireDateString = "";
        
        File.Delete("token.bin");
    }
}