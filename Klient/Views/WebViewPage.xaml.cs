using Klient.ViewModels;
using Microsoft.UI.Xaml.Controls;
namespace Klient.Views;

public sealed partial class WebViewPage : Page {
    public WebViewViewModel ViewModel {
        get;
    }

    public WebViewPage() {
        ViewModel = App.GetService<WebViewViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }
}
