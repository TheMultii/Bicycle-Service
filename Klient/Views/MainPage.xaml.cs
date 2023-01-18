using Klient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class MainPage : Page {
    
    public MainViewModel ViewModel {
        get;
    }

    public MainPage() {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void LoginButtonClick(object sender, RoutedEventArgs e) {
        ViewModel.LoginButtonClick(sender, e, ViewModel);
    }

    private void RegisterButtonClick(object sender, RoutedEventArgs e) {
        ViewModel.RegisterButtonClick(sender, e, ViewModel);
    }

    private void LogoutButtonClick(object sender, RoutedEventArgs e) {
        MainViewModel.LogoutButtonClick(sender, e, ViewModel);
    }
}
