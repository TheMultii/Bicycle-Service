using Klient.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class ContentGridPage : Page {

    public ContentGridViewModel ViewModel {
        get;
    }

    public ContentGridPage() {
        ViewModel = App.GetService<ContentGridViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
