using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Borelli_GestionaleVacanze {
    public class Piatto {
        private static bool _elim;
        private static string _nome;
        private static double _prezzo;
        private static byte _posiz;
        private static List<string> _ingredienti = new List<string>();

        public static bool Eliminato {
            get {
                return _elim;
            }
            set {
                _elim = value;
            }
        }
        public static string Nome {
            get {
                return _nome;
            }
            set {
                InserisciSeStringaValida(ref _nome, value, "Nome");
            }
        }
        public static double Prezzo {
            get {
                return _prezzo;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _prezzo, value, "Prezzo", float.MaxValue);
            }
        }
        public static byte Posizione {
            get {
                return _posiz;
            }
            set {
                double temp = Posizione;
                SettaSeMaggioreDiZeroMinoreDiMax(ref temp, value, "Posizione", 5);
                _posiz = (byte)temp;
            }
        }
        public static string[] Ingredienti {
            get {
                return _ingredienti.ToArray();
            }
            set {
                _ingredienti = new List<string>(value);
            }
        }


        public static string IngredientiToString(char separatore) {
            string temp = "";
            for (int i = 0; i < _ingredienti.Count; i++) {
                temp += $"{_ingredienti[i]}{separatore}";
            }
            if (temp != "") { /*tolgo la vigola finale*/
                temp = temp.Substring(0, temp.Length - 1);
            }

            return temp;
        }
        private static void InserisciSeStringaValida(ref string campo, string val, string perErrore) {
            if (!String.IsNullOrWhiteSpace(val)) {
                campo = val;
            } else {
                throw new Exception($"Inserire il campo \"{perErrore}\" valido");
            }
        }

        private static void SettaSeMaggioreDiZeroMinoreDiMax(ref double campo, double val, string nomeCampo, float max) {
            if (val >= 0 && val < max) {
                campo = val;
            } else {
                throw new Exception($"Il campo \"{nomeCampo}\" deve essere maggiore uguale a 0 e minore di {max}");
            }
        }
    }
}
