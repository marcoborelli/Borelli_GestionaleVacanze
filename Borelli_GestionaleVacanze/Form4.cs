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

namespace Borelli_GestionaleVacanze
{
    public partial class Form4 : Form
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

        public struct piatto
        {
            public bool eliminato;
            public string nome;
            public double prezzo;
            public string[] ingredienti;
            public int posizione;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Enter)) //tasto salva
                button1.PerformClick();
            else if (keyData == (Keys.NumPad1|Keys.Control) || keyData == (Keys.D1 | Keys.Control))//antipasto
                checkBox1.Checked = true;
            else if (keyData == (Keys.NumPad2 | Keys.Control) || keyData == (Keys.D2 | Keys.Control))//primo
                checkBox2.Checked = true;
            else if (keyData == (Keys.NumPad3 | Keys.Control) || keyData == (Keys.D3 | Keys.Control))//secondo
                checkBox3.Checked = true;
            else if (keyData == (Keys.NumPad4 | Keys.Control) || keyData == (Keys.D4 | Keys.Control))//dolce
                checkBox4.Checked = true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public int posizione { get; set; }
        public bool modificaAggiungi { get; set; }
        public int nummm { get; set; }
        public bool giaEliminato { get; set; }
        public bool ClienteProprietario { get; set; }

        string filename = @"piatti.ristorante", fileNumRecord = @"recordUsati.txt";
        int record = 128;

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            if (!ClienteProprietario)
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
                checkBox4.Enabled = false;
                button1.Text = "ESCI";
            }

            dimensioniRecord campiRecord;
            piatto piattino;

            campiRecord.padEliminato = 5;
            campiRecord.padNome = 15;
            campiRecord.padPrezzo = 10;
            campiRecord.padIngredienti = 20;
            campiRecord.padPosizione = 1;

            string riga;
            string[] fields;
            string[] ingredienti;
            //MessageBox.Show($"{posizione}");
            if (modificaAggiungi)//true=modifica false=aggiungi
            {
                var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                p.Seek(posizione, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(p, encoding))
                {
                    riga = reader.ReadString();
                    fields = riga.Split(';');
                }
                p.Close();
                ingredienti = fields[3].Split(',');

                piattino = AssegnaAStruct(fields, ingredienti);

                textBox1.Text = EliminaSpazi(piattino.nome);
                textBox2.Text = EliminaSpazi($"{piattino.prezzo}");
                textBox3.Text = EliminaSpazi(piattino.ingredienti[0]);
                textBox4.Text = EliminaSpazi(piattino.ingredienti[1]);
                textBox5.Text = EliminaSpazi(piattino.ingredienti[2]);
                textBox6.Text = EliminaSpazi(piattino.ingredienti[3]);

                if (piattino.posizione == 0)
                    checkBox1.Checked = true;
                else if (piattino.posizione == 1)
                    checkBox2.Checked = true;
                else if (piattino.posizione == 2)
                    checkBox3.Checked = true;
                else if (piattino.posizione == 3)
                    checkBox4.Checked = true;
            }


            //MessageBox.Show($"iNVECE NELLA 4 È {modificaAggiungi}");
        }
        private void button1_Click(object sender, EventArgs e)//salva
        {
            piatto piattino;
            dimensioniRecord campiRecord;

            campiRecord.padEliminato = 5;
            campiRecord.padNome = 15;
            campiRecord.padPrezzo = 10;
            campiRecord.padIngredienti = 20;
            campiRecord.padPosizione = 1;

            textBox2.Text = textBox2.Text.Replace(".", ",");//così mi accetta anche la virgola nel double

            string helo;
            //helo.

            string error = CampiValidi(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text, checkBox1, checkBox2, checkBox3, checkBox4, campiRecord);

            if (error == null)
            {
                int posizionee = NumDaCheckBox(checkBox1, checkBox2, checkBox3, checkBox4);//ottengo il numero da mettere come ultimo parametro
                piattino = InserireInStructValori(campiRecord, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text, posizionee, giaEliminato);
                ScriviFile(piattino, campiRecord, record, posizione, modificaAggiungi, filename, fileNumRecord, nummm, encoding);
                //record=lungh. record; posizione= pos. puntatore già sulla riga giusta; modificaAggiungi è il bool della form 3; nummm è il numero scritto sul file

                this.Close();
            }
            else
                MessageBox.Show(error);

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//antipasto
        {
            if (checkBox1.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox3, checkBox4);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)//primo
        {
            if (checkBox2.Checked)
                SelezionaSoloUnaCheckBox(checkBox1, checkBox3, checkBox4);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)//secondo
        {
            if (checkBox3.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox1, checkBox4);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)//dolce
        {
            if (checkBox4.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox3, checkBox1);
        }
        public static string CampiValidi(string nome, string prezzo, string ing1, string ing2, string ing3, string ing4, CheckBox uno, CheckBox due, CheckBox tre, CheckBox quattro, dimensioniRecord dimRecc)
        {
            string error = null;

            if (nome.Length > dimRecc.padNome || ing1.Length > dimRecc.padIngredienti || ing2.Length > dimRecc.padIngredienti || ing3.Length > dimRecc.padIngredienti || ing4.Length > dimRecc.padIngredienti)//controllo nome e ingredienti
                return error = "Inserire nei campi dei valori validi";

            if (!uno.Checked && !due.Checked && !tre.Checked && !quattro.Checked) //controllo che almeno uno sia selezionato
                return error = "Selezionare il tipo di portata";

            try//controllo prezzo
            {
                double helo = double.Parse(prezzo);
            }
            catch
            {
                return error = "Inserire un prezzo valido";
            }

            return error;
        }
        public static void AumentaFileContRecord(string fileNumRecord, int numm, Encoding encoding)
        {
            var U = new FileStream(fileNumRecord, FileMode.Create, FileAccess.ReadWrite);
            //using (StreamReader read = new StreamReader(U))
            //{
            numm++;
            using (StreamWriter write = new StreamWriter(U, encoding))
            {
                write.Write($"{numm}");
            }
            //} non credo che sta parte serva
            U.Close();
        }
        public static void ScriviFile(piatto piatt, dimensioniRecord dimm, int record, int pos, bool modificaAggiungi, string filename, string fileNumRecord, int numInFile, Encoding encoding)
        {
            string ingr = "";

            for (int i = 0; i < piatt.ingredienti.Length; i++)
                ingr += $"{piatt.ingredienti[i]},";
            ingr = ingr.Substring(0, ingr.Length - 1);//tolgo la virgola finale

            string tot = $"{$"{piatt.eliminato}".PadRight(dimm.padEliminato)};{piatt.nome};{$"{piatt.prezzo}".PadRight(dimm.padPrezzo)};{ingr};{$"{piatt.posizione}".PadRight(dimm.padPosizione)};".PadRight(record - 1);
            //non metto il toUpper ovunque perchè l'ho già messo prima dove potevo

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(pos, SeekOrigin.Begin);
            //Encoding cp437 = Encoding.GetEncoding(437);
            using (BinaryWriter writer = new BinaryWriter(f, encoding))
            {
                writer.Write(tot);
            }
            f.Close();

            if (!modificaAggiungi) //se aggiungo
                AumentaFileContRecord(fileNumRecord, numInFile, encoding);
        }
        public static piatto InserireInStructValori(dimensioniRecord dim, string nome, string prezzo, string ing1, string ing2, string ing3, string ing4, int pos, bool giaEliminato)
        {
            piatto piattuccino;
            piattuccino.ingredienti = new string[4];

            if (!giaEliminato) //se è false è perchè di là sto modifcando/creando un piatto che già esiste
                piattuccino.eliminato = true;
            else
                piattuccino.eliminato = false;

            piattuccino.nome = nome.PadRight(dim.padNome).ToUpper();
            piattuccino.prezzo = double.Parse(prezzo);
            piattuccino.ingredienti[0] = ing1.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[1] = ing2.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[2] = ing3.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[3] = ing4.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.posizione = pos;
            return piattuccino;
        }
        public static int NumDaCheckBox(CheckBox uno, CheckBox due, CheckBox tre, CheckBox quattro)
        {
            int pos = -1;

            if (uno.Checked)
                pos = 0;
            else if (due.Checked)
                pos = 1;
            else if (tre.Checked)
                pos = 2;
            else if (quattro.Checked)
                pos = 3;

            return pos;
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
        public static piatto AssegnaAStruct(string[] campi, string[] ingredienti)
        {
            piatto piattuccio;
            piattuccio.ingredienti = new string[ingredienti.Length];

            piattuccio.eliminato = bool.Parse(campi[0]);
            piattuccio.nome = campi[1];
            piattuccio.prezzo = double.Parse(campi[2]);

            for (int i = 0; i < ingredienti.Length; i++)
                piattuccio.ingredienti[i] = ingredienti[i];

            piattuccio.posizione = int.Parse(campi[4]);

            return piattuccio;
        }
        public static void SelezionaSoloUnaCheckBox(CheckBox uno, CheckBox due, CheckBox tre)
        {
            uno.Checked = false;
            due.Checked = false;
            tre.Checked = false;
        }
    }
}
