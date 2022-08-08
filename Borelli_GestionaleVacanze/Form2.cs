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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string errore = verificaValido(textBox1.Text, textBox2.Text);
            if (errore != null)
                MessageBox.Show(errore);
            else
            {
                using (StreamWriter writeCliente = new StreamWriter(@"utente.cliente",true))
                {
                    writeCliente.WriteLine(textBox1.Text);
                    writeCliente.WriteLine(textBox2.Text);
                }
                MessageBox.Show("Utente aggiunto con successo");
                this.Close();
            }
            
        }

        public static string verificaValido(string nomeUtente, string password)
        {
            string errore = null, rigaLetta = "";

            var J = new FileStream(@"utente.cliente", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader readCliente = new StreamReader(J))
            {
                do
                {
                    rigaLetta = readCliente.ReadLine();

                    if (rigaLetta == nomeUtente)
                        errore = "Username cliente già esistente";

                    readCliente.ReadLine();//vado a capo perchè non mi interessa la password
                } while (rigaLetta != null && errore == null);
            }
            J.Close();

            var H = new FileStream(@"utente.proprietario", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader readProprietario = new StreamReader(H))
            {
                do
                {
                    rigaLetta = readProprietario.ReadLine();

                    if (rigaLetta == nomeUtente)
                        errore = "Username proprietario già esistente";

                    readProprietario.ReadLine();//vado a capo perchè non mi interessa la password
                } while (rigaLetta != null && errore == null);
            }
            H.Close();


            return errore;
        }
    }
}
