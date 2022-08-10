﻿using System;
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
        public int nummm { get; set; }

        string filename = @"piatti.ristorante",fileNumRecord=@"recordUsati.txt";
        int record = 128;

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
            piatto piattino;
            dimensioniRecord campiRecord;

            campiRecord.padEliminato = 5;
            campiRecord.padNome = 15;
            campiRecord.padPrezzo = 10;
            campiRecord.padIngredienti = 20;
            campiRecord.padPosizione = 1;

            int posizionee = NumDaCheckBox(checkBox1, checkBox2, checkBox3, checkBox4);//ottengo il numero da mettere come ultimo parametro
            piattino = InserireInStructValori(campiRecord, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text, posizionee);
            ScriviFile(piattino, campiRecord, record, posizione, modificaAggiungi, filename, fileNumRecord, nummm);
            //record=lungh. record; posizione= pos. puntatore già sulla riga giusta; modificaAggiungi è il bool della form 3; nummm è il numero scritto sul file

            this.Close();
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

        public static void AumentaFileContRecord(string fileNumRecord, int numm)
        {
            var U = new FileStream(fileNumRecord, FileMode.Create, FileAccess.ReadWrite);
            //using (StreamReader read = new StreamReader(U))
            //{
                numm++;
                using (StreamWriter write = new StreamWriter(U))
                {
                    write.Write($"{numm}");
                }
            //} non credo che sta parte serva
            U.Close();
        }
        public static void ScriviFile(piatto piatt,dimensioniRecord dimm, int record, int pos, bool modificaAggiungi, string filename, string fileNumRecord, int numInFile)
        {
            string ingr = "";

            for (int i = 0; i < piatt.ingredienti.Length; i++)
                ingr += $"{piatt.ingredienti[i]},";
            ingr = ingr.Substring(0, ingr.Length - 1);//tolgo la virgola finale

            string tot = $"{$"{piatt.eliminato}".PadRight(dimm.padEliminato)};{piatt.nome};{$"{piatt.prezzo}".PadRight(dimm.padPrezzo)};{ingr};{$"{piatt.posizione}".PadRight(dimm.padPosizione)};".PadRight(record-1);
            //non metto il toUpper ovunque perchè l'ho già messo prima dove potevo

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(pos, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(f))
            {
                writer.Write(tot);
            }
            f.Close();

            if (!modificaAggiungi) //se aggiungo
                AumentaFileContRecord(fileNumRecord, numInFile);
        }
        public static piatto InserireInStructValori (dimensioniRecord dim, string nome, string prezzo, string ing1, string ing2, string ing3, string ing4, int pos)
        {
            piatto piattuccino;
            piattuccino.ingredienti = new string[4];

            piattuccino.eliminato = true;
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