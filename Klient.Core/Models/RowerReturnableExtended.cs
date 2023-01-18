﻿using Klient.Core.Model;

namespace Klient.Core.Models {
    public class RowerReturnableExtended: RowerReturnable {
        public string NewestStatus {
            get => Status.FirstOrDefault() ?? "Brak statusu";
        }
        public int StatusCount {
            get => Status.Count;
        }
    }
}