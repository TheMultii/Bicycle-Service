namespace Serwer {
    public class Zamówienie {
        private long _id;
        private string _imieKlienta;
        private string _nazwiskoKlienta;
        private Rower _rower;
        private DateTime _dataZamówienia;
        private DateTime _dataOdbioru;
        private IEnumerable<string> _statusZamówienia;

        public Zamówienie(long id, string imieKlienta, string nazwiskoKlienta, Rower rower, DateTime dataZamówienia, DateTime dataOdbioru, IEnumerable<string>? statusZamówienia = null) {
            _id = id;
            _imieKlienta = imieKlienta;
            _nazwiskoKlienta = nazwiskoKlienta;
            _rower = rower;
            _dataZamówienia = dataZamówienia;
            _dataOdbioru = dataOdbioru;
            _statusZamówienia = statusZamówienia ?? new List<string>() { "Nowe zamówienie" };
        }

        public Zamówienie(string imieKlienta, string nazwiskoKlienta, Rower rower, DateTime dataZamówienia, DateTime dataOdbioru, IEnumerable<string>? statusZamówienia = null) {
            _id = 0;
            _imieKlienta = imieKlienta;
            _nazwiskoKlienta = nazwiskoKlienta;
            _rower = rower;
            _dataZamówienia = dataZamówienia;
            _dataOdbioru = dataOdbioru;
            _statusZamówienia = statusZamówienia ?? new List<string>() { "Nowe zamówienie" };
        }

        public long ID {
            get => _id;
            set => _id = value;
        }

        public string ImieKlienta {
            get => _imieKlienta;
            set => _imieKlienta = value;
        }

        public string NazwiskoKlienta {
            get => _nazwiskoKlienta;
            set => _nazwiskoKlienta = value;
        }

        public Rower Rower {
            get => _rower;
            set => _rower = value;
        }

        public DateTime DataZamówienia {
            get => _dataZamówienia;
            set => _dataZamówienia = value;
        }

        public DateTime DataOdbioru {
            get => _dataOdbioru;
            set => _dataOdbioru = value;
        }

        public IEnumerable<string> StatusZamówienia {
            get => _statusZamówienia;
            set => _statusZamówienia = value;
        }

        public override string ToString() {
            return $"ID: {_id}, Imię klienta: {_imieKlienta}, Nazwisko klienta: {_nazwiskoKlienta}, Rower: {_rower}, Data zamówienia: {_dataZamówienia}, Data odbioru: {_dataOdbioru}, Status zamówienia: {_statusZamówienia}";
        }

        public void DodajStatus(string status) {
            var list = _statusZamówienia.ToList();
            list.Add(status);
            _statusZamówienia = list;
        }

    }
}
