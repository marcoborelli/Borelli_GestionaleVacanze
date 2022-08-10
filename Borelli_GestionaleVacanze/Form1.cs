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
        string nomeUtente,passwd;
        int login=-1;
        bool text1Testo = false, text2Testo = false;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox2.Focus();
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
            Form3 prova=new Form3();
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
                MessageBox.Show("PROPRIETARIO");
            else if (login == 2)
                MessageBox.Show("CLIENTE");
            else if (login == -1)
                MessageBox.Show("Nome utente o password errati");

            prova.Show();
            this.Hide();
        }
    }
}
