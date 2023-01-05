namespace Serwer {
    public class Rower {
        private string _marka;
        private string _model;
        private int _rokProdukcji;
        private double _cena;
        private string _kolor;
        private string _typ;

        public Rower(string marka, string model, int rokProdukcji, double cena, string kolor, string typ) {
            _marka = marka;
            _model = model;
            _rokProdukcji = rokProdukcji;
            _cena = cena;
            _kolor = kolor;
            _typ = typ;
        }

        public string Marka {
            get => _marka;
            set => _marka = value;
        }

        public string Model {
            get => _model;
            set => _model = value;
        }

        public int RokProdukcji {
            get => _rokProdukcji;
            set => _rokProdukcji = value;
        }

        public double Cena {
            get => _cena;
            set => _cena = value;
        }

        public string Kolor {
            get => _kolor;
            set => _kolor = value;
        }

        public string Typ {
            get => _typ;
            set => _typ = value;
        }

        public override string ToString() {
            return $"Marka: {_marka}, Model: {_model}, Rok produkcji: {_rokProdukcji}, Cena: {_cena}, Kolor: {_kolor}, Typ: {_typ}";
        }
    }
}
