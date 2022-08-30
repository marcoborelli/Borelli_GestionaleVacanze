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
    public partial class Form1 : Form
    {
        Encoding encoding = Encoding.GetEncoding(1252);

        string nomeUtente, passwd;
        string testoText1 = "Username", testoText2 = "Password";
        int login = -1;
        bool text1Testo = false, text2Testo = false;
        bool darkMode = false;
        string filenameSettings = @"settings.impostasiu", filenamePiatti = @"piatti.ristorante";
        int record = 128;

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = testoText1;
            textBox2.Text = testoText2;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Enter))
                button2.PerformClick();

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
            text1Testo = text2Testo = false;
            bool riscrivi = false;

            var p = new FileStream(filenameSettings, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader impostasiùRead = new StreamReader(p))
            {
                for (int i = 0; i < 2; i++)//due volte perchè ormai le impostazioni sono due: 1 dark mode 2 scrivi o no ordine su file
                {
                    string ciHoMessoMezzoraAcapireLerrore = impostasiùRead.ReadLine();
                    if (ciHoMessoMezzoraAcapireLerrore != "False" && ciHoMessoMezzoraAcapireLerrore != "True")
                        riscrivi = true;
                }

            }
            p.Close();

            if (riscrivi)
            {
                var y = new FileStream(filenameSettings, FileMode.Create, FileAccess.ReadWrite);
                y.Close();

                using (StreamWriter impostasiùWrite = new StreamWriter(filenameSettings))
                {
                    for (int i = 0; i < 2; i++)
                        impostasiùWrite.WriteLine("False");
                    darkMode = false;
                }
            }
            else
            {
                using (StreamReader impostasiùRead = new StreamReader(filenameSettings))//inverto darkmode/altra mode
                {
                    darkMode = bool.Parse(impostasiùRead.ReadLine());
                }
            }

            if (darkMode)
            {
                button1.BackColor = button2.BackColor = textBox1.BackColor = textBox2.BackColor = Color.FromArgb(37, 42, 64);
                textBox1.BorderStyle = textBox2.BorderStyle = BorderStyle.FixedSingle;
                button1.ForeColor = button2.ForeColor = textBox1.ForeColor = textBox2.ForeColor = Color.White;
                this.BackColor = Color.FromArgb(46, 51, 73);
            }
            else
            {
                button1.BackColor = button2.BackColor = textBox1.BackColor = textBox2.BackColor = Color.White;
                textBox1.BorderStyle = textBox2.BorderStyle = BorderStyle.Fixed3D;
                button1.ForeColor = button2.ForeColor = textBox1.ForeColor = textBox2.ForeColor = Color.Black;
                this.BackColor = Form1.DefaultBackColor;
            }
        }
        private void button1_Click(object sender, EventArgs e) //nuova utenza
        {
            Form2 registrazione = new Form2();
            registrazione.Show();
        }
        void prova_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = true;
            login = -1;
            text1Testo = text2Testo = false;
            textBox1.Text = testoText1;
            textBox2.Text = testoText2;
            Form1_Load(sender, e);
        }
        private void button2_Click(object sender, EventArgs e) //login
        {
            Form3 prova = new Form3();
            prova.FormClosed += new FormClosedEventHandler(prova_FormClosed);
            int nRigheProp = 0, nRigheClien = 0;

            var p = new FileStream(@"utente.proprietario", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            p.Close();
            using (StreamReader readProp = new StreamReader(@"utente.proprietario", true))
            {
                while (readProp.ReadLine() != null)
                    nRigheProp++;
            }

            var c = new FileStream(@"utente.cliente", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            c.Close();
            using (StreamReader readClien = new StreamReader(@"utente.cliente", true))
            {
                while (readClien.ReadLine() != null)
                    nRigheClien++;
            }

            if (nRigheClien % 2 != 0)//perchè una riga è username altra password quindi devono essere pari
            {
                MessageBox.Show("C'è un errore nel file password del cliente. Risolvere");
                Application.Exit();
            }
            else if (nRigheProp % 2 != 0)
            {
                MessageBox.Show("C'è un errore nel file password del proprietario. Risolvere");
                Application.Exit();
            }


            string rigaLetta;

            nomeUtente = textBox1.Text;
            passwd = textBox2.Text;

            var Q = new FileStream(@"utente.proprietario", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader read = new StreamReader(Q))
            {
                if (nomeUtente == read.ReadLine())//ora sto solo controllando per il proprietario
                {
                    if (passwd == read.ReadLine())
                        login = 1;
                }
                else
                {
                    var J = new FileStream(@"utente.cliente", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    using (StreamReader readCliente = new StreamReader(J))
                    {
                        do
                        {
                            rigaLetta = readCliente.ReadLine();
                            if (nomeUtente == rigaLetta)
                            {
                                if (passwd == readCliente.ReadLine())//perchè solo se il nome utente è giusto lui è qui dentro quindi deve leggere la riga sotto sennò non entra più
                                    login = 2;
                            }
                        } while (rigaLetta != null && login != 2);//se ci sono più utenti

                    }
                    J.Close();
                }
            }
            Q.Close();

            if (login == 1)
            {
                prova.ClienteProprietario = true;//true=proprietario
                prova.Text = "PROPRIETARIO";
                prova.Show();
                this.Hide();
            }
            else if (login == 2)
            {
                prova.ClienteProprietario = false;//false=cliente
                prova.Text = "CLIENTE";
                prova.Show();
                this.Hide();
            }
            else if (login == -1)
                MessageBox.Show("Nome utente o password errati");

        }
        private void textBox1_MouseClick(object sender, MouseEventArgs e)//nome utente
        {
            if (!text1Testo)
                textBox1.Text = "";
        }
        private void textBox2_MouseClick(object sender, MouseEventArgs e)//password
        {
            if (!text2Testo)
                textBox2.Text = "";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            text1Testo = true;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            text2Testo = true;
        }
    }
}
