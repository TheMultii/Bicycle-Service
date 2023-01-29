using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using IdGen;
using Klient.Behaviors;
using Klient.Contracts.ViewModels;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Klient.Views;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;

namespace Klient.ViewModels;

public class ListDetailsViewModel : ObservableRecipient, INavigationAware {

    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    private static readonly string myDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myData.json");

    private string _token = string.Empty;
    public string Token {
        get => _token;
        set => SetProperty(ref _token, value);
    }

    private UserMyDataDTO? _userDataDTO = null;
    public UserMyDataDTO? UserDataDTO {
        get => _userDataDTO;
        set => SetProperty(ref _userDataDTO, value);
    }

    private bool _isLoading = false;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// check if user is allowed to modify an order (has permission "Service" or "Shop")
    /// </summary>
    private bool _isAllowedToModifyOrder = false;
    public bool IsAllowedToModifyAnOrder {
        get => _isAllowedToModifyOrder;
        set => SetProperty(ref _isAllowedToModifyOrder, value);
    }

    private static readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IdStructure structure = new(41, 10, 12);
    private static readonly IdGeneratorOptions options = new(structure, new DefaultTimeSource(epoch));
    private static readonly IdGenerator generator = new(0, options);
    
    private RowerExtended? _selected;
    public RowerExtended? Selected {
        get => _selected;
        set {
            SetProperty(ref _selected, value);
        }
    }

    public ObservableCollection<RowerExtended> AllUsersOrders { get; private set; } = new();

    public void UpdateAllUsersOrders(RowerExtended item) {
        var index = -1;
        for (int i = 0; i < AllUsersOrders.Count; i++) {
            if (AllUsersOrders[i].Uid == item.Uid) {
                index = i;
                break;
            }
        }
        if (index != -1) {
            Selected = item;
            AllUsersOrders[index] = item;
            Selected = item;
        }
    }
    
    public ListDetailsViewModel() {
        try {
            using StreamReader reader = new(new FileStream(myDataPath, FileMode.Open, FileAccess.ReadWrite));
            UserDataDTO = JsonConvert.DeserializeObject<UserMyDataDTO?>(reader.ReadToEnd());
        } catch (Exception) {
            UserDataDTO = null;
        }
        if (UserDataDTO == null) return;
        try {
            IsAllowedToModifyAnOrder = UserDataDTO.Permission == "Service" || UserDataDTO.Permission == "Shop";
            if (!IsAllowedToModifyAnOrder) return;
            using BinaryReader reader = new(new FileStream(tokenPath, FileMode.Open, FileAccess.ReadWrite));
            _token = reader.ReadString();
            Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
            if (Core.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization")) {
                Core.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
            }
            Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", "Bearer " + _token);
        } catch (Exception) { }
    }

    public async void OnNavigatedTo(object parameter) => await GetAllUsersOrdersMethod();

    public async void RefreshData() => await GetAllUsersOrdersMethod();

    public void OnNavigatedFrom() {
    }

    public void EnsureItemSelected() {
        Selected ??= AllUsersOrders.First();
    }

    private async Task GetAllUsersOrdersMethod() {
        if (Token == string.Empty) return;
        if (UserDataDTO == null) return;
        if (UserDataDTO.Permission != "Service" && UserDataDTO.Permission != "Shop") return;

        AllUsersOrders.Clear();
        
        if (Token == string.Empty) return;
        IsLoading = true;

        IEnumerable<RowerExtended> _orders = (await GetAllUsersOrders()).Reverse();
        
        foreach (RowerExtended _order in _orders) {
            AllUsersOrders.Add(_order);
        }
        IsLoading = false;
    }

    private static async Task<IEnumerable<RowerExtended>> GetAllUsersOrders() {
        try {
            ISerwisRowerowyApi serwisRowerowyApi = new SerwisRowerowyApi();
            List<Rower> _orders = await serwisRowerowyApi.ApiServiceBicyclesGetAsync();

            List<RowerExtended> _ordersExtended = new();
            foreach (Rower order in _orders) {
                DateTime createdAt = DateTime.MinValue;

                try {
                    Id id = generator.FromId(order.Uid ?? 0);
                    createdAt = id.DateTimeOffset.ToLocalTime().DateTime;
                } catch (Exception) { }

                order.Status.Reverse();

                _ordersExtended.Add(new RowerExtended {
                    Uid = order.Uid,
                    Owner = order.Owner,
                    Brand = order.Brand,
                    Model = order.Model,
                    Type = order.Type,
                    Price = order.Price,
                    Status = order.Status,
                    CreatedAt = createdAt
                });
            }
            return _ordersExtended;
        } catch (Exception) { }

        return new List<RowerExtended>();
    }
}