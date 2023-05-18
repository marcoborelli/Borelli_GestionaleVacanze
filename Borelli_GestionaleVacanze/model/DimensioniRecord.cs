using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borelli_GestionaleVacanze {
    public class DimensioniRecord {
        private static int _padEliminato, _padNome, _padPrezzo, _padIngredienti, _padPosizione, _record;

        /*properties*/
        public static int PadEliminato {
            get {
                return _padEliminato;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _padEliminato, value, "Pad Eliminato", int.MaxValue);
            }

        }
        public static int PadNome {
            get {
                return _padNome;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _padNome, value, "Pad Nome", int.MaxValue);
            }
        }
        public static int PadPrezzo {
            get {
                return _padPrezzo;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _padPrezzo, value, "Pad Prezzo", int.MaxValue);
            }
        }
        public static int PadIngredienti {
            get {
                return _padIngredienti;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _padIngredienti, value, "Pad Ingredienti", int.MaxValue);
            }
        }
        public static int PadPosizione {
            get {
                return _padPosizione;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _padPosizione, value, "Pad Posizione", int.MaxValue);
            }
        }
        public static int Record {
            get {
                return _record;
            }
            set {
                SettaSeMaggioreDiZeroMinoreDiMax(ref _record, value, "Record", int.MaxValue);
            }
        }
        /*fine properties*/

        private static void SettaSeMaggioreDiZeroMinoreDiMax(ref int campo, int val, string nomeCampo, int max) {
            if (val > 0 && val < max) {
                campo = val;
            } else {
                throw new Exception($"Il campo \"{nomeCampo}\" deve essere maggiore uguale a 0 e minore di {max}");
            }
        }
    }
}
