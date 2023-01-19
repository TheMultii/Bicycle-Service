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

    private void MakeAnOrderCancel(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.MakeAnOrderCancel(sender, e);
    }

    private void MakeAnOrderSubmit(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.MakeAnOrderSubmit(sender, e);
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        ViewModel.MakeOnOrderComboBox_SelectionChanged(sender, e);
    }

    private void CheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.MakeOnOrderCheckBox_Checked(sender, e);
    }

    private void CheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ViewModel.MakeOnOrderCheckBox_Unchecked(sender, e);
    }
}
