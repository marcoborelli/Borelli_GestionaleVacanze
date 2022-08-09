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
    public partial class Form2 : Form
    {
        bool text1Testo = false, text2Testo=false;
        string testoText1 = "Nuovo username", testoText2 = "Nuova password";
        public Form2()
        {
            InitializeComponent();
            textBox1.Text = testoText1;
            textBox2.Text = testoText2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string errore = verificaValido(textBox1.Text, textBox2.Text,testoText1, testoText2);
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

        public static string verificaValido(string nomeUtente, string password, string text1Predef, string text2Predef)
        {
            string errore = null, rigaLetta = "";
            int numSpaziUtente = 0, numSpaziPasswd = 0;

            if (nomeUtente == "" || password == "")
            {
                errore = "Inserire valori validi in tutti i campi";
                return errore;
            }

            for (int i = 0; i < nomeUtente.Length; i++) //controllo che non siano solo spazi
            {
                if (nomeUtente.Substring(i, 1) == " ")
                    numSpaziUtente++;
            }
            for (int i = 0; i < password.Length; i++)
            {
                if (password.Substring(i, 1) == " ")
                    numSpaziPasswd++;
            }
            if (password.Length == numSpaziPasswd || nomeUtente.Length == numSpaziUtente)
            {
                errore = "Non è possibile inserire solo spazi nei campi";
                return errore;
            }

            if (nomeUtente == text1Predef || password == text2Predef) //se sono uguali ai testi originali
            {
                errore = "Immettere dei valori diversi da quelli di default";
                return errore;
            }

            var J = new FileStream(@"utente.cliente", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader readCliente = new StreamReader(J))
            {
                do
                {
                    rigaLetta = readCliente.ReadLine();

                    if (rigaLetta == nomeUtente)
                    {
                        errore = "Username cliente già esistente";
                        return errore;
                    }

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
                    {
                        errore = "Username proprietario già esistente";
                        return errore;
                    }
                    readProprietario.ReadLine();//vado a capo perchè non mi interessa la password
                } while (rigaLetta != null && errore == null);
            }
            H.Close();

            return errore;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!text1Testo)
                textBox1.Text = ""; //metto vero solo quando ci ho scritto testo sennò ogni volta che premo cancella
        }

        private void textBox1_TextChanged(object sender, EventArgs e) 
        {
            text1Testo = true;
        }
        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (!text2Testo)
                textBox2.Text = "";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            text2Testo = true;
        }
    }
}
