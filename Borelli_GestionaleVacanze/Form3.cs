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
    public partial class Form3 : Form
    {
        int record = 128, numm = 0;
        string filename = @"piatti.ristorante";
        bool modifica = false;
        public Form3()
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            listView1.Columns.Add("Visible", 0);
            listView1.Columns.Add("NOME", 140);
            listView1.Columns.Add("PREZZO", 60);
            listView1.Columns.Add("INGREDIENTI", 250);
            listView1.Columns.Add("POSIZIONE", 100);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            StampaElementi(listView1, filename);

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
                //MessageBox.Show($"'{listView1.SelectedItems[0].SubItems[1].Text}'");
                modifica = true;
                button1_Click(sender, e);

            }

        }
        private void button1_Click(object sender, EventArgs e) //aggiunta piatto
        {
            Form4 ModificaAggiungi = new Form4();
            if (modifica)
                ModificaAggiungi.posizione = cercaPiatto(listView1.SelectedItems[0].SubItems[1].Text, filename) - record;//-record perchè lui mi da il numero quando ha finito dileggere riga quindi torno a inizio
            else
                ModificaAggiungi.posizione = record * numm;

            ModificaAggiungi.modificaAggiungi = modifica;//metto il bool nella form 4 uguale a questo bool

            modifica = false;
            //listBox1.ClearSelected();//deseleziono
            ModificaAggiungi.nummm = numm;

            ModificaAggiungi.Show();
            Form3_Load(sender, e);
        }

        public static int cercaPiatto(string nomePiatto, string filename)
        {
            int pos = -1;
            bool corrispondenza = true;
            string riga = "";
            string[] fields;
            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p);
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

        public static void StampaElementi(ListView listino, string filename)
        {
            string riga;
            string[] fields;
            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(0, SeekOrigin.Begin);
            using (BinaryReader leggiNomi = new BinaryReader(f))
            {
                while (f.Position < f.Length)
                {
                    riga = leggiNomi.ReadString();
                    fields = riga.Split(';');
                    if (bool.Parse(fields[0]))
                    {
                        ListViewItem item = new ListViewItem(fields);
                        listino.Items.Add(item);
                    }
                    
                }
            }
            f.Close();
        }
    }
}
