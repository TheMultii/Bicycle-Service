namespace Serwer {
    public class Rower {
        public long UID {
            get; set;
        }

        public User Owner {
            get; set;
        }

        public string Brand {
            get; set;
        }

        public string Model {
            get; set;
        }

        public string Type {
            get; set;
        }

        public double Price {
            get; set;
        }

        public List<RowerStatus> Status {
            get; set;
        }

        public Rower(long uid, User owner, string brand, string model, string type, double price, List<RowerStatus> status) {
            UID = uid;
            Owner = owner;
            Brand = brand;
            Model = model;
            Type = type;
            Price = price;
            Status = status;
        }
        public Rower(User owner, string brand, string model, string type, double price, List<RowerStatus> status) {
            UID = 0;
            Owner = owner;
            Brand = brand;
            Model = model;
            Type = type;
            Price = price;
            Status = status;
        }
    }

    public class RowerStatus {
        public long UID {
            get; set;
        }

        public User Changed_by {
            get; set;
        }

        public string Status {
            get; set;
        }

        public RowerStatus(long uid, User changed_by, string status) {
            UID = uid;
            Changed_by = changed_by;
            Status = status;
        }
        public RowerStatus(User changed_by, string status) {
            UID = 0;
            Changed_by = changed_by;
            Status = status;
        }
    }

    public class RowerDTO {
        public string Brand {
            get; set;
        } = string.Empty;

        public string Model {
            get; set;
        } = string.Empty;

        public string Type {
            get; set;
        } = string.Empty;

        public double Price {
            get; set;
        } = 0.0;
    }

    public class RowerStatusDTO {
        public string Status {
            get; set;
        } = string.Empty;
    }

    public class RowerReturnable {
        public long UID {
            get; set;
        } = 0;

        public long OwnerUID {
            get; set;
        } = 0;

        public string Brand {
            get; set;
        } = string.Empty;

        public string Model {
            get; set;
        } = string.Empty;

        public string Type {
            get; set;
        } = string.Empty;

        public double Price {
            get; set;
        } = 0.0;
    }
}
