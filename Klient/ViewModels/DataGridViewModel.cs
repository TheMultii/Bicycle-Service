using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Klient.Contracts.ViewModels;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;

namespace Klient.ViewModels;

public class DataGridViewModel : ObservableRecipient, INavigationAware {
    
    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    private static readonly string myDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myData.json");

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

    private bool _isPlacingAnOrder = false;
    public bool IsPlacingAnOrder {
        get => _isPlacingAnOrder;
        set => SetProperty(ref _isPlacingAnOrder, value);
    }

    private bool _allowPlacingAnOrder = false;
    public bool AllowPlacingAnOrder {
        get => _allowPlacingAnOrder;
        set => SetProperty(ref _allowPlacingAnOrder, value);
    }

    private string _newOrderBrand = string.Empty;
    public string NewOrderBrand {
        get => _newOrderBrand;
        set {
            SetProperty(ref _newOrderBrand, value);
            CheckIfAllowedToPlaceAnOrder();
        }
    }

    private string _newOrderModel = string.Empty;
    public string NewOrderModel {
        get => _newOrderModel;
        set {
            SetProperty(ref _newOrderModel, value);
            CheckIfAllowedToPlaceAnOrder();
        }
    }

    private string _newOrderType = string.Empty;
    public string NewOrderType {
        get => _newOrderType;
        set {
            SetProperty(ref _newOrderType, value);
            CheckIfAllowedToPlaceAnOrder();
        }
    }

    private bool _newOrderChecked = false;
    public bool NewOrderChecked {
        get => _newOrderChecked;
        set {
            SetProperty(ref _newOrderChecked, value);
            CheckIfAllowedToPlaceAnOrder();
        }
    }

    private UserMyDataDTO? _userDataDTO = null;
    public UserMyDataDTO? UserDataDTO {
        get => _userDataDTO;
        set => SetProperty(ref _userDataDTO, value);
    }

    public ObservableCollection<RowerReturnable> Source { get; } = new();

    public DataGridViewModel() {
        try {
            using StreamReader reader3 = new(new FileStream(myDataPath, FileMode.Open, FileAccess.ReadWrite));
            UserDataDTO = JsonConvert.DeserializeObject<UserMyDataDTO?>(reader3.ReadToEnd());
        } catch (Exception) {
            UserDataDTO = null;
        }
        if (UserDataDTO == null) return;
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

    private static async Task<IEnumerable<RowerReturnableExtended>> GetUserRowery() {
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
        } catch (Exception) { }
        return new List<RowerReturnableExtended>();
    }

    private async Task GetUserRoweryMethod() {
        if (Token == string.Empty) return;
        if (UserDataDTO == null) return;
        if (UserDataDTO.Permission != "Customer") return;
        
        Source.Clear();

        if (_token == string.Empty) return;
        IsLoading = true;

        IEnumerable<RowerReturnable> data = await GetUserRowery();

        foreach (var item in data) {
            Source.Add(item);
        }
        IsLoading = false;
    }

    internal async void RefreshData(object sender, RoutedEventArgs e) {
        await GetUserRoweryMethod();
    }

    internal void MakeAnOrder(object sender, RoutedEventArgs e) {
        IsPlacingAnOrder = true;
    }

    internal void MakeAnOrderCancel(object sender, RoutedEventArgs e) {
        IsPlacingAnOrder = false;
    }

    internal void MakeAnOrderSubmit(object sender, RoutedEventArgs e) {
        if (!AllowPlacingAnOrder) return;
    }
    
    internal void MakeOnOrderComboBox_SelectionChanged(object sender, RoutedEventArgs e) {
        if (sender is ComboBox comboBox) {
            if (comboBox.SelectedValue != null) {
                NewOrderType = comboBox.SelectedValue.ToString() ?? "";
            }
        }
    }
    
    internal void MakeOnOrderCheckBox_Checked(object sender, RoutedEventArgs e) {
        NewOrderChecked = true;
    }

    internal void MakeOnOrderCheckBox_Unchecked(object sender, RoutedEventArgs e) {
        NewOrderChecked = false;
    }

    private void CheckIfAllowedToPlaceAnOrder() {
        if (NewOrderBrand.Trim().Length < 3 || NewOrderModel.Trim().Length < 3 ||
            NewOrderType.Trim().Length == 0 || !NewOrderChecked) {
            AllowPlacingAnOrder = false;
            return;
        }
        AllowPlacingAnOrder = true;
    }

    public void OnNavigatedFrom() {
        
    }
}