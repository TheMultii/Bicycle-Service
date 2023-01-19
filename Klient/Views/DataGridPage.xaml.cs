using Klient.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class DataGridPage : Page {
    public DataGridViewModel ViewModel {
        get;
    }

    public DataGridPage() {
        ViewModel = App.GetService<DataGridViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void RefreshData(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.RefreshData(sender, e);
    }

    private void MakeAnOrder(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.MakeAnOrder(sender, e);
    }
}
