using IdGen;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class ListDetailsDetailControl : UserControl {
    public RowerExtended? RowerItem {
        get => GetValue(RowerItemProperty) as RowerExtended;
        set {
            SetValue(RowerItemProperty, value);
            if (value != null) {
                CheckS1 = value.Status.Where(x => x.Status == s1).Any();
                CheckS2 = value.Status.Where(x => x.Status == s2).Any();
                CheckS3 = value.Status.Where(x => x.Status == s3).Any();
            } else {
                CheckS1 = false;
                CheckS2 = false;
                CheckS3 = false;
            }
            s1CheckName.IsEnabled = !CheckS1 && CheckS2 && CheckS3;
            s2CheckName.IsEnabled = !CheckS1 && !CheckS2 && CheckS3;
            s3CheckName.IsEnabled = !CheckS1 && !CheckS2 && !CheckS3;
            s4CheckName.IsEnabled = false;
            UpdateLayout();
        }
    }

    private static readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IdStructure structure = new(41, 10, 12);
    private static readonly IdGeneratorOptions options = new(structure, new DefaultTimeSource(epoch));
    private static readonly IdGenerator generator = new(0, options);

    public const string s1 = "Zrealizowane";
    public const string s2 = "Oczekujące";
    public const string s3 = "W trakcie realizacji";
    public const string s4 = "Nowe zamówienie";

    public bool CheckS1 = false;
    public bool CheckS2 = false;
    public bool CheckS3 = false;
    public bool CheckS4 = true;

    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    private readonly string _token = string.Empty;

    public static readonly DependencyProperty RowerItemProperty = DependencyProperty.Register("RowerItem", typeof(RowerExtended), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnRowerItemChanged));

    //listen to RowerItemProperty changes
    private static void OnRowerItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ListDetailsDetailControl control) {
            control.ForegroundElement.ChangeView(0, 0, 1);
            control.RowerItem = e.NewValue as RowerExtended;
        }
    }

    public ListDetailsDetailControl() {
        InitializeComponent();

        try {
            using BinaryReader reader = new(new FileStream(tokenPath, FileMode.Open, FileAccess.ReadWrite));
            _token = reader.ReadString();
            Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
            if (Core.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization")) {
                Core.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
            }
            Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer {_token}");
        } catch (Exception) { }

        RowerItem = GetValue(RowerItemProperty) as RowerExtended;
    }

    //TODO: Permission check before adding new status (service/shop)

    private async void CheckBox_CheckedS3(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (RowerItem?.Status.Where(s => s.Status == s3).Any() ?? false) {
                return;
            }
            await UpdateOrderStatusMethod(cb);
        }
    }

    private async void CheckBox_CheckedS2(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (RowerItem?.Status.Where(s => s.Status == s2).Any() ?? false) {
                return;
            }
            await UpdateOrderStatusMethod(cb);
        }
    }

    private async void CheckBox_CheckedS1(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (RowerItem?.Status.Where(s => s.Status == s2).Any() ?? false) {
                return;
            }
            await UpdateOrderStatusMethod(cb);
        }
    }

    private async Task UpdateOrderStatusMethod(CheckBox cb) {
        ISerwisRowerowyApi serwisRowerowyApi = new SerwisRowerowyApi();
        await serwisRowerowyApi.ApiServiceOrderUidStatusPutAsync(RowerItem?.Uid ?? 0, new RowerStatusDTO() {
            Status = cb.Content.ToString()
        });

        Rower order = await serwisRowerowyApi.ApiServiceBicyclesUidGetAsync(RowerItem?.Uid ?? 0);

        Id id = generator.FromId(RowerItem?.Uid ?? 0);
        DateTime cAt = id.DateTimeOffset.ToLocalTime().DateTime;
        order.Status.Reverse();
        RowerItem = new RowerExtended {
            Uid = order.Uid,
            Owner = order.Owner,
            Brand = order.Brand,
            Model = order.Model,
            Type = order.Type,
            Price = order.Price,
            Status = order.Status,
            CreatedAt = cAt
        };

        lViewContainer.ItemsSource = RowerItem.Status;
        UpdateLayout();
    }
}