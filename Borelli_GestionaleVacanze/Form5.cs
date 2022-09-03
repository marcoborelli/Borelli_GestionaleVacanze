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
    public partial class Form5 : Form
    {
        string filenameSettings = @"impostasiu.ristorante";
        public bool dark { get; set; }
        public bool salvaSuFile { get; set; }
        bool leggiSecondo = true;

        public Form5()
        {
            InitializeComponent();
        }
        private void Form5_Load(object sender, EventArgs e)
        {
            using (StreamReader impostasiùRead = new StreamReader(filenameSettings, false))
            {
                dark = bool.Parse(impostasiùRead.ReadLine());
                if (dark)
                {
                    button2.Text = "DARK MODE: OFF";
                    button2.BackColor = Color.FromArgb(37, 42, 64);
                    button2.ForeColor = checkBox1.ForeColor = Color.White;
                    this.BackColor = Color.FromArgb(46, 51, 73);
                }
                else
                {
                    button2.Text = "DARK MODE: ON";
                    button2.BackColor = Color.White;
                    button2.ForeColor = checkBox1.ForeColor = Color.Black;
                    this.BackColor = Form5.DefaultBackColor;
                }

                if (leggiSecondo)//perchè quando cambio dark mode rifaccio file. Mi darebbe errore quindi non lo rileggo
                    checkBox1.Checked = bool.Parse(impostasiùRead.ReadLine());
            }
            leggiSecondo = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            bool helo = false; //variabile temp
            using (StreamReader impostasiùRead = new StreamReader(filenameSettings))//inverto darkmode/altra mode
            {
                helo = bool.Parse(impostasiùRead.ReadLine());
            }
            using (StreamWriter impostasiùWrite = new StreamWriter(filenameSettings))
            {
                impostasiùWrite.WriteLine($"{Inverti(helo)}");
            }
            leggiSecondo = false;
            Form5_Load(sender, e);
        }
        private void Form5_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBox1.Checked)
                salvaSuFile = true;
            else
                salvaSuFile = false;

            var uu = new FileStream(filenameSettings, FileMode.Create, FileAccess.Write);
            uu.Close();

            var p = new FileStream(filenameSettings, FileMode.Append, FileAccess.Write);
            using (StreamWriter impostasiùWrite = new StreamWriter(p))
            {
                impostasiùWrite.WriteLine(dark);
                impostasiùWrite.WriteLine(salvaSuFile);
            }
            p.Close();

            e.Cancel = true;
            this.Hide();
        }
        public static bool Inverti(bool helo)
        {
            return !helo;
        }
    }
}
