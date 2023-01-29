using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Klient.Core.Api;
using Klient.Core.Model;
using Newtonsoft.Json;
using Klient.Helpers;

namespace Klient.ViewModels;

public class MainViewModel : ObservableRecipient {

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private UserMyDataDTO? _userDataDTO = null;
    public UserMyDataDTO? UserDataDTO {
        get => _userDataDTO;
        set => SetProperty(ref _userDataDTO, value);
    }
    
    //paths
    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    private static readonly string tokenExpirePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tokenExpire.bin");
    private static readonly string myDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myData.json");

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
            using BinaryReader reader = new(new FileStream(tokenPath, FileMode.Open, FileAccess.ReadWrite));
            Token = reader.ReadString();

            using BinaryReader reader2 = new(new FileStream(tokenExpirePath, FileMode.Open, FileAccess.ReadWrite));
            TokenExpireDate = DateTime.Parse(reader2.ReadString());
            TokenExpireDateString = $"Twój token wygaśnie {TokenExpireDate:dd.MM.yyyy HH:mm:ss}.";
            
            if (TokenExpireDate < DateTime.Now) {
                Token = string.Empty;
                TokenExpireDate = DateTime.MinValue;
                TokenExpireDateString = "";

                File.Delete(tokenPath);
                File.Delete(tokenExpirePath);
                File.Delete(myDataPath);
                return;
            }

            using StreamReader reader3 = new(new FileStream(myDataPath, FileMode.Open, FileAccess.ReadWrite));
            UserDataDTO = JsonConvert.DeserializeObject<UserMyDataDTO?>(reader3.ReadToEnd());

        } catch (Exception) { }
    }

    internal async void RegisterButtonClick(object sender, RoutedEventArgs e) {
        string errorMessage = string.Empty;
        bool displayError = false;

        if (_register_login.Length < 3 || _register_login.Length > 100) {
            errorMessage = "Login musi zawierać od 3 do 100 znaków";
            displayError = true;
        } else if (_register_password.Length < 3 || _register_password.Length > 100) {
            errorMessage = "Hasło musi zawierać od 3 do 100 znaków";
            displayError = true;
        }
        if (displayError) {
            await DisplayError.show(sender, errorMessage);
            return;
        }
        IsLoading = true;

        Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
        IUserApi userApi = new UserApi();

        UserRegisterDTO userRegisterDTO = new() {
            Username = _register_login,
            Password = _register_password,
            Name = _register_name,
            Surname = _register_surname
        };

        try {
            string _token_response = await userApi.ApiUserRegisterPutAsync(userRegisterDTO);
            if (_token_response == null) {
                await DisplayError.show(sender, "Nie udało się zarejestrować użytkownika");
                IsLoading = false;
                return;
            }
            
            await PostLoginRegisterFunction(_token_response, userApi);
            IsLoading = false;

        } catch (Exception ex) {
            await DisplayError.show(sender, ex.Message);
            IsLoading = false;
            return;
        }

        IsLoading = false;
    }

    private async Task PostLoginRegisterFunction(string _token_response, IUserApi userApi) {
        Token = _token_response.Replace("\"", string.Empty);
        DateTime tomorrow = DateTime.Now.AddDays(1);
        TokenExpireDateString = $"Twój token wygaśnie {tomorrow:dd.MM.yyyy HH:mm:ss}.";
        TokenExpireDate = DateTime.Now.AddDays(1);
        Login = "";
        Password = "";

        if (Core.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization")) {
            Core.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
        }
        Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer {Token}");
        UserMyDataDTO myData = await userApi.ApiUserInfoGetAsync();
        UserDataDTO = myData;
        string myDataJSON = JsonConvert.SerializeObject(myData);

        using FileStream fs = new(tokenPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using FileStream fs2 = new(tokenExpirePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        using FileStream fs3 = new(myDataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        using BinaryWriter writer = new(fs);
        using BinaryWriter writer2 = new(fs2);
        using StreamWriter writer3 = new(fs3);

        RegisterLogin = "";
        RegisterName = "";
        RegisterSurname = "";
        RegisterPassword = "";
        RegisterPasswordConfirm = "";
        Login = "";
        Password = "";

        writer.Write(Token);
        writer2.Write(tomorrow.ToString("dd.MM.yyyy HH:mm:ss"));

        if (myData != null) {
            await writer3.WriteAsync(myDataJSON);
        }

    }

    internal async void LoginButtonClick(object sender, RoutedEventArgs e) {
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
            await DisplayError.show(sender, errorMessage);
            return;
        }

        IsLoading = true;
        Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
        IUserApi userApi = new UserApi();
        UserLoginDTO userLoginDTO = new() { Username = _login, Password = _password };
        try {
            string _token_response = await userApi.ApiUserLoginPostAsync(userLoginDTO);
            if (_token_response != null) {
                await PostLoginRegisterFunction(_token_response, userApi);
                IsLoading = false;
            }
        } catch (Exception) {
            await DisplayError.show(sender, "Niepoprawne dane");
        }
        IsLoading = false;
    }
    
    internal void LogoutButtonClick(object sender, RoutedEventArgs e) {
        Token = string.Empty;
        TokenExpireDate = DateTime.MinValue;
        TokenExpireDateString = "";
        
        File.Delete(tokenPath);
        File.Delete(tokenExpirePath);
        File.Delete(myDataPath);
    }
}