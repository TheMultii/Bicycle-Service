﻿using CommunityToolkit.WinUI.UI.Controls;

using Klient.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class ListDetailsPage : Page {
    public ListDetailsViewModel ViewModel {
        get;
    }

    public ListDetailsPage() {
        ViewModel = App.GetService<ListDetailsViewModel>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e) {
        try {
            if (e == ListDetailsViewState.Both) {
                ViewModel.EnsureItemSelected();
            }
        } catch (Exception) { }
    }
}
