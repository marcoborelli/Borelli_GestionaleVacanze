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
        int record = 128;
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //listBox1.Items.Add("HElO");
            StampaElementi(listBox1, @"voti.mosco");
        }
        public static void StampaElementi(ListBox listino, string filename)
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
                        listino.Items.Add(fields[1]);
                }
            }
            f.Close();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
