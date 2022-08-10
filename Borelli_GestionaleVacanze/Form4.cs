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

        public struct elementiForm
        {
            public TextBox nome;
            public TextBox prezzo;
            public TextBox ingrediente1;
            public TextBox ingrediente2;
            public TextBox ingrediente3;
            public TextBox ingrediente4;
            public CheckBox antipasto;
            public CheckBox primo;
            public CheckBox secondo;
            public CheckBox dolce;
        }

        public int posizione { get; set; }
        public bool modificaAggiungi { get; set; }

        string filename = @"piatti.ristorante";

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
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
                using (BinaryReader reader = new BinaryReader(p))
                {
                    riga = reader.ReadString();
                    fields = riga.Split(';');
                }
                p.Close();
                ingredienti = fields[3].Split(',');

                piattino= AssegnaAStruct(fields, ingredienti);

                textBox1.Text = EliminaSpazi(piattino.nome);
                textBox2.Text = EliminaSpazi($"{piattino.prezzo}");
                textBox3.Text = EliminaSpazi(piattino.ingredienti[0]);
                textBox4.Text = EliminaSpazi(piattino.ingredienti[1]);
                textBox5.Text = EliminaSpazi(piattino.ingredienti[2]);
                textBox6.Text = EliminaSpazi(piattino.ingredienti[3]);

                if (piattino.posizione == 0)
                    checkBox1.Checked=true;
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
