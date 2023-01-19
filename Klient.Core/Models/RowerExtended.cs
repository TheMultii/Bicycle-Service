using Klient.Core.Model;

namespace Klient.Core.Models {
    public class RowerExtended : Rower {
        public DateTime CreatedAt { get; set; }

        public string NewestStatus {
            get => Status.FirstOrDefault().Status ?? "Brak statusu";
        }
        public int StatusCount {
            get => Status.Count;
        }
        
        public string OwnerString {
            get => $"{Owner.Name} {Owner.Surname}";
        }
        
        public string BicycleString {
            get => Capitalize($"{Brand} {Model} — {Type}");
        }

        private static string Capitalize(string str) {
            if (string.IsNullOrEmpty(str)) return str;
            string[] words = str.Split(' ');
            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                if (word.Length > 1) {
                    words[i] = char.ToUpper(word[0]) + word.Substring(1);
                } else {
                    words[i] = word.ToUpper();
                }
            }
            return string.Join(" ", words);
        }
    }
}
