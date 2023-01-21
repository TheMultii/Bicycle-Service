using CommunityToolkit.WinUI.UI.Controls;
using Klient.Core.Models;
using Klient.ViewModels;

using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace Klient.Views;

public sealed partial class ListDetailsPage : Page {
    public ListDetailsViewModel ViewModel {
        get;
    }

    public ListDetailsPage() {
        ViewModel = App.GetService<ListDetailsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e) {
        try {
            if (e == ListDetailsViewState.Both) {
                ViewModel.EnsureItemSelected();
            }
        } catch (Exception) { }
    }

    internal void UpdateAllUsersOrders(RowerExtended rI) {
        ViewModel.UpdateAllUsersOrders(rI);
    }
}
