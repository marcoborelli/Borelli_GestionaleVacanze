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
        public int posizione { get; set; }
        public bool modificaAggiungi { get; set; }

        string filename = @"piatti.ristorante";

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            string riga;
            string[] fields;
            string[] ingredienti;
            MessageBox.Show($"{posizione}");
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
                textBox1.Text = fields[1];
                textBox2.Text = fields[2];
                textBox3.Text = ingredienti[0];
                textBox4.Text = ingredienti[1];
                textBox5.Text = ingredienti[2];
                textBox6.Text = ingredienti[3];

            }

            //MessageBox.Show($"iNVECE NELLA 4 È {modificaAggiungi}");
        }

    }
}
