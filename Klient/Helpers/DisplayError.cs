using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klient.Helpers {
    public static class DisplayError {
        public static async Task<ContentDialogResult> show(object sender, string errorMessage, string errorTitle = "Błąd", string errorButtonText = "OK") {
            ContentDialog dialog = new() {
                XamlRoot = ((Control)sender).XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = errorTitle,
                Content = errorMessage,
                PrimaryButtonText = errorButtonText,
                DefaultButton = ContentDialogButton.Primary
            };

            return await dialog.ShowAsync();
        }
    }
}
