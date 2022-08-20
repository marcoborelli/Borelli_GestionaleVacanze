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
        string nomeUtente, passwd;
        int login = -1;
        bool text1Testo = false, text2Testo = false;
        bool darkMode = false;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            bool riscrivi = false;

            var p = new FileStream(@"dark.Impostasiu", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader impostasiùRead = new StreamReader(p))
            {
                string ciHoMessoMezzoraAcapireLerrore = impostasiùRead.ReadLine();
                if (ciHoMessoMezzoraAcapireLerrore != "False" && ciHoMessoMezzoraAcapireLerrore != "True")
                    riscrivi = true;

            }
            p.Close();

            if (riscrivi)
            {
                var y = new FileStream(@"dark.Impostasiu", FileMode.Create, FileAccess.ReadWrite);
                y.Close();

                using (StreamWriter impostasiùWrite = new StreamWriter(@"dark.Impostasiu"))
                {
                    impostasiùWrite.WriteLine("False");
                    darkMode = false;
                }
            }
            else
            {
                using (StreamReader impostasiùRead = new StreamReader(@"dark.Impostasiu"))//inverto darkmode/altra mode
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


        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Enter))
                button2.PerformClick();

            return base.ProcessCmdKey(ref msg, keyData);
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

        private void button1_Click(object sender, EventArgs e) //nuova utenza
        {
            Form2 registrazione = new Form2();
            registrazione.Show();
        }

        private void button2_Click(object sender, EventArgs e) //login
        {
            Form3 prova = new Form3();
            int nRigheProp = 0, nRigheClien = 0;

            using (StreamReader readProp = new StreamReader(@"utente.proprietario", true))
            {
                while (readProp.ReadLine() != null)
                    nRigheProp++;
            }

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
    }
}
