﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Klient.Contracts.ViewModels;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Newtonsoft.Json.Linq;

namespace Klient.ViewModels;

public class DataGridViewModel : ObservableRecipient, INavigationAware {

    private string _token = string.Empty;
    public string Token {
        get => _token;
        set => SetProperty(ref _token, value);
    }

    public bool test = false;

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<RowerReturnable> Source { get; } = new();

    public DataGridViewModel() {
        try {
            using BinaryReader reader = new(new FileStream("token.bin", FileMode.Open, FileAccess.ReadWrite));
            _token = reader.ReadString();
            Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
            Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", "Bearer " + _token);
        } catch (Exception) { }
    }

    public async void OnNavigatedTo(object parameter) {
        Source.Clear();

        if (_token == string.Empty) return;
        IsLoading = true;

        IEnumerable<RowerReturnable> data = await GetUserRowery();

        foreach (var item in data) {
            Source.Add(item);
        }
        IsLoading = false;
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

    public void OnNavigatedFrom() {
    }
}