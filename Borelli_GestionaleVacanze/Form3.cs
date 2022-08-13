using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Text.RegularExpressions;

namespace Borelli_GestionaleVacanze
{
    public partial class Form3 : Form
    {
        Encoding encoding = Encoding.GetEncoding(1252);
        public struct dimensioniRecord
        {
            public int padEliminato;
            public int padNome;
            public int padPrezzo;
            public int padIngredienti;
            public int padPosizione;
        }

        int record = 128, numm = 0;
        string filename = @"piatti.ristorante";
        bool modifica = false;
        public Form3()
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.FullRowSelect = true;


            listView1.Columns.Add("NOME", 140);
            listView1.Columns.Add("PREZZO", 60);
            listView1.Columns.Add("INGREDIENTI", 300);
            listView1.Columns.Add("POSIZIONE", 100);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            //StampaElementi(listView1, filename, false, "");
            textBox1_TextChanged(sender, e);

            var W = new FileStream(@"recordUsati.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader read = new StreamReader(W))
            {
                string num = read.ReadLine();
                if (num == null)
                {
                    using (StreamWriter write = new StreamWriter(W))
                    {
                        write.Write("0");
                    }
                }
                else
                {
                    try
                    {
                        numm = int.Parse(num);
                    }
                    catch
                    {
                        MessageBox.Show("Errore file 'RecordUsati.txt' il programma si chiuderà");
                        Application.Exit();
                    }
                }
            }
            W.Close();
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                //MessageBox.Show($"'{listView1.SelectedItems[0].Text}'");
                modifica = true;
                button1_Click(sender, e);
            }

        }
        private void button1_Click(object sender, EventArgs e) //aggiunta piatto
        {
            Form4 ModificaAggiungi = new Form4();
            if (modifica)
                ModificaAggiungi.posizione = cercaPiatto(listView1.SelectedItems[0].Text, filename, encoding) - record;//-record perchè lui mi da il numero quando ha finito dileggere riga quindi torno a inizio
            else
                ModificaAggiungi.posizione = record * numm;

            ModificaAggiungi.modificaAggiungi = modifica;//metto il bool nella form 4 uguale a questo bool

            modifica = false;
            //listBox1.ClearSelected();//deseleziono
            ModificaAggiungi.nummm = numm;

            ModificaAggiungi.Show();
        }
        private void Form3_Activated(object sender, EventArgs e)
        {
            Form3_Load(sender, e);
        }

        private void button2_Click(object sender, EventArgs e) //eliminazione logica piatto
        {
            dimensioniRecord campiRecord;

            campiRecord.padEliminato = 5;
            campiRecord.padNome = 15;
            campiRecord.padPrezzo = 10;
            campiRecord.padIngredienti = 20;
            campiRecord.padPosizione = 1;

            int inizioRecord = cercaPiatto(listView1.SelectedItems[0].Text, filename, encoding) - record;
            eliminaOripristinaPiatti(inizioRecord, false, filename, record, campiRecord, encoding);

            Form3_Load(sender, e);
        }
        private void textBox1_TextChanged(object sender, EventArgs e) //textBox ricerca
        {
            if (textBox1.Text == "" || textBox1.Text == null)
                StampaElementi(listView1, filename, false, "", encoding);
            else
                StampaElementi(listView1, filename, true, textBox1.Text.ToUpper(), encoding);

        }
        private void button4_Click(object sender, EventArgs e)//elimina fisicamente
        {
            DialogResult dialog = MessageBox.Show("Così facendo perderai definitivamente i piatti eliminati in precedenza. SIcuro di volerlo fare?", "ELIMINAZIONE FISICA", MessageBoxButtons.YesNo);
            if (dialog == DialogResult.Yes)
                EliminaDefinitivamente(filename, numm, record, encoding);
        }

        public static void EliminaDefinitivamente(string filename, int numm, int record, Encoding encoding)
        {
            int nVolte = 0, posPunt = 0;
            string rigaTemp = "";
            bool dentroTesto = false;

            int[] indiciEliminati = new int[numm];

            trovaEliminati(indiciEliminati, filename, record, numm, encoding);

            for (int i = 0; i < indiciEliminati.Length; i++)
            {
                if (indiciEliminati[i] != -1)
                    nVolte++;
            }


            for (int i = 0; i < nVolte; i++)
            {
                trovaEliminati(indiciEliminati, filename, record, numm, encoding);
                posPunt = indiciEliminati[0];
                do
                {
                    var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    dentroTesto = false;

                    if (posPunt + record < record * numm)
                    {
                        dentroTesto = true;
                        p.Seek(posPunt + record, SeekOrigin.Begin);
                        using (BinaryReader reader = new BinaryReader(p, encoding))
                        {
                            rigaTemp = reader.ReadString();
                        }
                        p.Close();
                        Console.WriteLine($"RIGA TEMP: '{rigaTemp}'");
                        var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        y.Seek(posPunt, SeekOrigin.Begin);
                        using (BinaryWriter writer = new BinaryWriter(y, encoding))
                        {
                            writer.Write(rigaTemp);
                        }
                        y.Close();
                        posPunt += record;
                    }
                    else
                    {
                        p.Close();
                    }

                } while (dentroTesto);

                var U = new FileStream(@"recordUsati.txt", FileMode.Create, FileAccess.ReadWrite);
                using (StreamReader read = new StreamReader(U))
                {
                    numm--;
                    using (StreamWriter write = new StreamWriter(U))
                    {
                        write.Write($"{numm}");
                    }
                }
                U.Close();
            }


            var fileNuovo = new FileStream(@"menu.piattoTemp", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var fileOriginale = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            BinaryReader vecchio = new BinaryReader(fileOriginale, encoding);
            BinaryWriter nuovo = new BinaryWriter(fileNuovo, encoding);

            fileOriginale.Seek(0, SeekOrigin.Begin);

            while (fileOriginale.Position < record * numm)
            {
                string temp;
                temp = vecchio.ReadString();
                nuovo.Write(temp);
            }

            fileNuovo.Close();
            fileOriginale.Close();


            SovrascrivereFile(@"menu.piattoTemp", filename);
        }
        public static void SovrascrivereFile(string fileTemp, string fileOrig)
        {
            FileInfo fi = new FileInfo(fileTemp);
            FileInfo newFi = new FileInfo(fileOrig);
            newFi.Delete();
            newFi = fi.CopyTo(fileOrig);
            fi.Delete();
        }
        public static void trovaEliminati(int[] indici, string filename, int record, int cosiUsati, Encoding encoding)
        {
            string riga = "";
            string[] fields;
            int indUsati = 0;
            for (int i = 0; i < indici.Length; i++)
                indici[i] = -1;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            while (p.Position < record * cosiUsati)
            {
                riga = reader.ReadString();
                fields = riga.Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (!bool.Parse(fields[0]))
                {
                    indici[indUsati] = Convert.ToInt32(p.Position) - record;
                    indUsati++;
                }
            }
            p.Close();
        }
        public static void eliminaOripristinaPiatti(int inizioRecord, bool eliminaRipristina, string filename, int record, dimensioniRecord lunghRec, Encoding encoding)
        {
            string[] fields;
            string riga;
            bool eliminato;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            p.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryReader reader = new BinaryReader(p, encoding))
            {
                riga = reader.ReadString();
                fields = riga.Split(';');
            }
            p.Close();

            if (eliminaRipristina)//si ripristina
                eliminato = true;
            else
                eliminato = false;

            var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            y.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(y, encoding))
            {
                string totale = $"{$"{eliminato}".PadRight(lunghRec.padEliminato)};{fields[1]};{fields[2]};{fields[3]};{fields[4]};".PadRight(record - 1);
                //Console.WriteLine($"NEW: '{totale}' {totale.Length}\nOLD: '{riga}' {riga.Length}\nRECORD: {record}");
                //Console.ReadKey();
                writer.Write(totale);
            }
            y.Close();
        }
        public static void StampaElementi(ListView listino, string filename, bool ricerca, string testoRicerca, Encoding encoding)
        {
            listino.Items.Clear();
            string riga;

            string[] fields;
            string[] fieldsRidotti;
            string[] ingredienti;

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(0, SeekOrigin.Begin);
            using (BinaryReader leggiNomi = new BinaryReader(f, encoding))
            {
                while (f.Position < f.Length)
                {
                    riga = leggiNomi.ReadString();
                    fields = riga.Split(';');

                    fieldsRidotti = new string[fields.Length - 1]; //così non stampo a video il bool dell'eleminazione
                    for (int i = 0; i < fields.Length - 1; i++)
                        fieldsRidotti[i] = fields[i + 1];

                    ingredienti = fieldsRidotti[2].Split(','); //per togliere spazi in eccesso
                    fieldsRidotti[2] = "";
                    for (int i = 0; i < ingredienti.Length; i++)
                    {
                        ingredienti[i] = $"{EliminaSpazi(ingredienti[i])}";
                        fieldsRidotti[2] += $"{ingredienti[i]}, ";
                    }
                    fieldsRidotti[2] = fieldsRidotti[2].Substring(0, fieldsRidotti[2].Length - 2);

                    if (int.Parse(fieldsRidotti[3]) == 0)
                        fieldsRidotti[3] = "ANTIPASTO";
                    else if (int.Parse(fieldsRidotti[3]) == 1)
                        fieldsRidotti[3] = "PRIMO";
                    else if (int.Parse(fieldsRidotti[3]) == 2)
                        fieldsRidotti[3] = "SECONDO";
                    else if (int.Parse(fieldsRidotti[3]) == 3)
                        fieldsRidotti[3] = "DOLCE";

                    Regex rx = new Regex(testoRicerca);

                    if ((bool.Parse(fields[0]) && !ricerca) || (ricerca && rx.IsMatch(fieldsRidotti[0])&& bool.Parse(fields[0])))
                    {
                        ListViewItem item = new ListViewItem(fieldsRidotti);
                        listino.Items.Add(item);
                    }
                }
            }
            f.Close();

        }
        public static string EliminaSpazi(string elemento)
        {
            int i = elemento.Length;
            bool spazio = true;
            while (i > 0 && spazio)
            {
                if (elemento.Substring(i - 1, 1) == " ")
                    elemento = elemento.Substring(0, elemento.Length - 1);
                else
                    spazio = false;
                i--;
            }
            return elemento;
        }
        public static int cercaPiatto(string nomePiatto, string filename, Encoding encoding)
        {
            int pos = -1;
            bool corrispondenza = true;
            string riga = "";
            string[] fields;
            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            p.Seek(0, SeekOrigin.Begin);

            while (p.Position < p.Length && corrispondenza)
            {
                riga = reader.ReadString();
                fields = riga.Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (bool.Parse(fields[0]) && fields[1] == nomePiatto)//ora il fatto che sia true non serve a molto perchè io seleziono tra i piatti veri ma lo lascio lo stesso
                {
                    corrispondenza = true;
                    pos = Convert.ToInt32(p.Position);
                }
            }
            p.Close();

            return pos;
        }
    }
}
