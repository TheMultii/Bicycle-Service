using Klient.Core.Models;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Klient.Views;

public sealed partial class ListDetailsDetailControl : UserControl {
    public RowerExtended? RowerItem {
        get => GetValue(RowerItemProperty) as RowerExtended;
        set => SetValue(RowerItemProperty, value);
    }

    public static readonly DependencyProperty RowerItemProperty = DependencyProperty.Register("RowerItem", typeof(RowerExtended), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnRowerItemChanged));

    public ListDetailsDetailControl() {
        InitializeComponent();
    }

    private static void OnRowerItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is ListDetailsDetailControl control) {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
