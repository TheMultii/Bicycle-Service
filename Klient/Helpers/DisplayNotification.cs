using Klient.Contracts.Services;
using Microsoft.UI.Xaml;

namespace Klient.Helpers {
    public static class DisplayNotification {

        private static T GetService<T>() where T : class {
            if ((Application.Current as App)!.Host.Services.GetService(typeof(T)) is not T service) {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }
        
        public static void Show(string title, string description) {
            GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory, title, description));
        }
    }
}
