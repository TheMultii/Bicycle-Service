using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Klient.Core.Api;
using Klient.Core.Model;

namespace Klient.ViewModels;

public class MainViewModel : ObservableRecipient {

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

    //methods

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

    internal async void RegisterButtonClick(object sender, RoutedEventArgs e) {
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
            await DisplayError(sender, errorMessage);
            return;
        }

        Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
        Core.Client.Configuration.Default.AddDefaultHeader("Authorization", "Bearer eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwNjQxODY5MDg3MDEwOTc5ODQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVGhlTXVsdGlpIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ3VzdG9tZXIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL2V4cGlyYXRpb24iOiIxNy4wMS4yMDIzIDIxOjA3OjI4IiwiZXhwIjoxNjczOTg2MDQ4fQ.EY45_xlQFztSdoXWFTn6LOtTFBhpFxMI0CDc2fqPYQ6vXFdWSI-RTL5OdE05nCC9XltqXgq970Ale63xWLD97w");
        IUserApi userApi = new UserApi();
        ISerwisRowerowyApi serwisRowerowyApi = new SerwisRowerowyApi();
        string str_res = await userApi.ApiUserTestGetAsync();
        List<RowerReturnable> kek = await serwisRowerowyApi.MyOrdersGetAsync();
        str_res = str_res + " " + kek.Count;


        ContentDialog dialog2 = new() {
            XamlRoot = ((Button)sender).XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "Pytańsko",
            Content = str_res,
            PrimaryButtonText = "Tak",
            DefaultButton = ContentDialogButton.Primary
        };

        var result2 = await dialog2.ShowAsync();

        //if (result == ContentDialogResult.Primary) {
        //    dialog.Content = "Kliknięto: Tak";
        //} else if (result == ContentDialogResult.Secondary) {
        //    dialog.Content = "Kliknięto: Nie";
        //} else {
        //    dialog.Content = "Kliknięto: Muszę sprawdzić";
        //}

        //await dialog.ShowAsync();

    }

}
