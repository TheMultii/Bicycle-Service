using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Klient.Contracts.ViewModels;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Microsoft.UI.Xaml;

namespace Klient.ViewModels;

public class DataGridViewModel : ObservableRecipient, INavigationAware {
    
    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    
    private string _token = string.Empty;
    public string Token {
        get => _token;
        set => SetProperty(ref _token, value);
    }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<RowerReturnable> Source { get; } = new();

    public DataGridViewModel() {
        try {
            using BinaryReader reader = new(new FileStream(tokenPath, FileMode.Open, FileAccess.ReadWrite));
            _token = reader.ReadString();
            Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
            if (Core.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization")) {
                Core.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
            }
            Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", "Bearer " + _token);
        } catch (Exception) { }
    }

    public async void OnNavigatedTo(object parameter) {
        await GetUserRoweryMethod();
    }

    private async Task<IEnumerable<RowerReturnableExtended>> GetUserRowery() {
        ISerwisRowerowyApi serwisRowerowyApi = new SerwisRowerowyApi();
        try {
            List<RowerReturnable> rowery = await serwisRowerowyApi.MyOrdersGetAsync();
            List<RowerReturnableExtended> roweryExtended = new();
            foreach (var rower in rowery) {
                roweryExtended.Add(new RowerReturnableExtended {
                    Uid = rower.Uid,
                    OwnerUID = rower.OwnerUID,
                    Brand = rower.Brand,
                    Model = rower.Model,
                    Type = rower.Type,
                    Price = rower.Price,
                    Status = rower.Status
                });
            }
            return roweryExtended;
        } catch (Exception) {
            //
        }
        return new List<RowerReturnableExtended>();
    }

    private async Task GetUserRoweryMethod() {
        if (Token == string.Empty) return;
        Source.Clear();

        if (_token == string.Empty) return;
        IsLoading = true;

        IEnumerable<RowerReturnable> data = await GetUserRowery();

        foreach (var item in data) {
            Source.Add(item);
        }
        IsLoading = false;
    }

    internal async void RefreshData(object sender, RoutedEventArgs e, DataGridViewModel vm) {
        await GetUserRoweryMethod();
    }

    public void OnNavigatedFrom() {
    }
}