using Klient.Core.Model;
using System.Globalization;

namespace Klient.Core.Models {
    public class RowerReturnableExtended: RowerReturnable {
        public DateTime CreatedAt { get; set; }

        public string NewestStatus {
            get => Status.LastOrDefault() ?? "Brak statusu";
        }
        public int StatusCount {
            get => Status.Count;
        }
        public string PriceLocalizedString {
            get => string.Format(CultureInfo.CreateSpecificCulture("pl-PL"), "{0:C}", Price);
        }
    }
}
