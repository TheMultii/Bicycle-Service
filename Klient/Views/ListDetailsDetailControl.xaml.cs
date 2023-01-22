using IdGen;
using Klient.Core.Api;
using Klient.Core.Model;
using Klient.Core.Models;
using Klient.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using System.Globalization;

namespace Klient.Views;

public sealed partial class ListDetailsDetailControl : UserControl {
    public RowerExtended? RowerItem {
        get => GetValue(RowerItemProperty) as RowerExtended;
        set => SetValue(RowerItemProperty, value);
    }
    
    public RowerExtended? RI;

    public bool IsLoading { set => LoadingIndicator.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }

    internal void RUN() {
        RI = GetValue(RowerItemProperty) as RowerExtended;
        IsLoading = false;
        if (RI != null) {
            CheckS1 = RI.Status.Where(x => x.Status == s1).Any();
            CheckS2 = RI.Status.Where(x => x.Status == s2).Any();
            CheckS3 = RI.Status.Where(x => x.Status == s3).Any();
            lViewContainer.ItemsSource = RI.Status;
        } else {
            CheckS1 = false;
            CheckS2 = false;
            CheckS3 = false;
        }
        s1CheckName.IsChecked = CheckS1;
        s2CheckName.IsChecked = CheckS2;
        s3CheckName.IsChecked = CheckS3;
        s4CheckName.IsChecked = CheckS4;
        s1CheckName.IsEnabled = !CheckS1 && CheckS2 && CheckS3;
        s2CheckName.IsEnabled = !CheckS1 && !CheckS2 && CheckS3;
        s3CheckName.IsEnabled = !CheckS1 && !CheckS2 && !CheckS3;
        s4CheckName.IsEnabled = false;

        RI_BicycleString.Text = RI?.BicycleString;
        RI_UID.Text = RI?.Uid.ToString();
        RI_NewestStatus.Text = RI?.NewestStatus;
        RI_CreatedAt.Text = RI?.CreatedAt.ToString();
        RI_Price.Text = (string)doubleToPriceStringConverter.Convert(RI?.Price, null, 1, null);
        RI_Brand.Text = RI?.Brand;
        RI_Model.Text = RI?.Model;
        RI_Type.Text = RI?.Type;
    }

    private static readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IdStructure structure = new(41, 10, 12);
    private static readonly IdGeneratorOptions options = new(structure, new DefaultTimeSource(epoch));
    private static readonly IdGenerator generator = new(0, options);

    private static readonly DoubleToPriceStringConverter doubleToPriceStringConverter = new();

    public readonly string s1 = "Zrealizowane";
    public readonly string s2 = "Oczekujące";
    public readonly string s3 = "W trakcie realizacji";
    public readonly string s4 = "Nowe zamówienie";

    public bool CheckS1 = false;
    public bool CheckS2 = false;
    public bool CheckS3 = false;
    public bool CheckS4 = true;

    private static readonly string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.bin");
    private readonly string _token = string.Empty;

    private static readonly string myDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "myData.json");
    private UserMyDataDTO? _userDataDTO = null;

    public static readonly DependencyProperty RowerItemProperty = DependencyProperty.Register("RowerItem", typeof(RowerExtended), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnRowerItemChanged));

    //listen to RowerItemProperty changes
    private static void OnRowerItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ListDetailsDetailControl control) {
            control.ForegroundElement.ChangeView(0, 0, 1);
            control.RUN();
        }
    }

    public ListDetailsDetailControl() {
        InitializeComponent();
        RI = RowerItem;

        try {
            using BinaryReader reader = new(new FileStream(tokenPath, FileMode.Open, FileAccess.ReadWrite));
            _token = reader.ReadString();
            Core.Client.Configuration.Default.BasePath = "https://localhost:7050/";
            if (Core.Client.Configuration.Default.DefaultHeader.ContainsKey("Authorization")) {
                Core.Client.Configuration.Default.DefaultHeader.Remove("Authorization");
            }
            Core.Client.Configuration.Default.DefaultHeader.Add("Authorization", $"Bearer {_token}");
        } catch (Exception) { }

        try {
            using StreamReader reader = new(new FileStream(myDataPath, FileMode.Open, FileAccess.ReadWrite));
            string json = reader.ReadToEnd();
            _userDataDTO = JsonConvert.DeserializeObject<UserMyDataDTO>(json);
        } catch (Exception) { }

        RowerItem = GetValue(RowerItemProperty) as RowerExtended;
    }

    private async void CheckBox_CheckedS3(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (_userDataDTO == null || _userDataDTO.Permission != "Service") {
                cb.IsChecked = false;
                await DisplayError.show(sender, "Potrzebujsz uprawnień \"Service\", by móc zmienić status zamówienia na \"W trakcie realizacji\".", "Niewystarczające uprawnienia");
                return;
            }
            if (RI?.Status.Where(s => s.Status == s3).Any() ?? false) return;

            IsLoading = true;
            await UpdateOrderStatusMethod(cb);
            IsLoading = false;
        }
    }

    private async void CheckBox_CheckedS2(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (_userDataDTO == null || _userDataDTO.Permission != "Service") {
                cb.IsChecked = false;
                await DisplayError.show(sender, "Potrzebujsz uprawnień \"Service\", by móc zmienić status zamówienia na \"Oczekujące\".", "Niewystarczające uprawnienia");
                return;
            }
            if (RI?.Status.Where(s => s.Status == s2).Any() ?? false) return;

            IsLoading = true;
            await UpdateOrderStatusMethod(cb);
            IsLoading = false;
        }
    }

    private async void CheckBox_CheckedS1(object sender, RoutedEventArgs e) {
        if (sender is CheckBox cb) {
            if (_userDataDTO == null || _userDataDTO.Permission != "Shop") {
                cb.IsChecked = false;
                await DisplayError.show(sender, "Potrzebujesz uprawnień \"Shop\", by móc zmienić status zamówienia na \"Zrealizowane\".", "Niewystarczające uprawnienia");
                return;
            }
            if (RI?.Status.Where(s => s.Status == s1).Any() ?? false) return;

            IsLoading = true;
            await UpdateOrderStatusMethod(cb);
            IsLoading = false;
        }
    }

    private async Task UpdateOrderStatusMethod(CheckBox cb) {
        ISerwisRowerowyApi serwisRowerowyApi = new SerwisRowerowyApi();
        await serwisRowerowyApi.ApiServiceOrderUidStatusPutAsync(RI?.Uid ?? 0, new RowerStatusDTO() {
            Status = cb.Content.ToString()
        });

        Rower order = await serwisRowerowyApi.ApiServiceBicyclesUidGetAsync(RI?.Uid ?? 0);

        Id id = generator.FromId(RI?.Uid ?? 0);
        DateTime cAt = id.DateTimeOffset.ToLocalTime().DateTime;
        order.Status.Reverse();
        RI = new RowerExtended {
            Uid = order.Uid,
            Owner = order.Owner,
            Brand = order.Brand,
            Model = order.Model,
            Type = order.Type,
            Price = order.Price,
            Status = order.Status,
            CreatedAt = cAt
        };

        RI_BicycleString.Text = RI?.BicycleString;
        RI_UID.Text = RI?.Uid.ToString();
        RI_NewestStatus.Text = RI?.NewestStatus;
        RI_CreatedAt.Text = RI?.CreatedAt.ToString();
        DoubleToPriceStringConverter doubleToPriceStringConverter = new();
        RI_Price.Text = (string)doubleToPriceStringConverter.Convert(RI?.Price, null, 1, null);
        RI_Brand.Text = RI?.Brand;
        RI_Model.Text = RI?.Model;
        RI_Type.Text = RI?.Type;
        lViewContainer.ItemsSource = RI?.Status;

        if (RI == null) return;
        CheckS1 = RI.Status.Any(x => x.Status == s1);
        CheckS2 = RI.Status.Any(x => x.Status == s2);
        CheckS3 = RI.Status.Any(x => x.Status == s3);
        s1CheckName.IsChecked = CheckS1;
        s2CheckName.IsChecked = CheckS2;
        s3CheckName.IsChecked = CheckS3;
        s4CheckName.IsChecked = CheckS4;
        s1CheckName.IsEnabled = !CheckS1 && CheckS2 && CheckS3;
        s2CheckName.IsEnabled = !CheckS1 && !CheckS2 && CheckS3;
        s3CheckName.IsEnabled = !CheckS1 && !CheckS2 && !CheckS3;
        s4CheckName.IsEnabled = false;

        var parent = VisualTreeHelper.GetParent(this);
        while (parent != null && parent is not ListDetailsPage) {
            parent = VisualTreeHelper.GetParent(parent);
        }
        if (parent is ListDetailsPage page) {
            page.UpdateAllUsersOrders(RI);
        }
    }
}