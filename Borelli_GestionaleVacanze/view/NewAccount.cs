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
using System.Security.Cryptography;

namespace Borelli_GestionaleVacanze {
    public partial class NewAccount : Form {
        string testoText1 = "Nuovo username", testoText2 = "Nuova password";
        string filenameSettings = @"impostasiu.ristorante";
        bool darkMode = false;
        bool text1Testo = false, text2Testo = false;
        int volte = 0;
        public NewAccount() {
            InitializeComponent();
            textBox1.Text = testoText1;
            textBox2.Text = testoText2;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Back | Keys.Control))//turna andrè
                button2.PerformClick();
            else if (keyData == (Keys.Enter))//crea utente
                button1.PerformClick();

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void Form2_Load(object sender, EventArgs e) {
            volte++;
            text1Testo = false; //lo rimetto anche qui dentro perchè sennò col fatto che io di default nelle text box ci ketto la stringa questo diventa true
            text2Testo = false;
            using (StreamReader impostasiùRead = new StreamReader(filenameSettings, false)) {
                darkMode = bool.Parse(impostasiùRead.ReadLine());
                if (darkMode) {
                    button1.BackColor = button2.BackColor = button3.BackColor = button4.BackColor = textBox1.BackColor = textBox2.BackColor = Color.FromArgb(37, 42, 64);
                    button1.ForeColor = button2.ForeColor = button3.ForeColor = button4.ForeColor = textBox1.ForeColor = textBox2.ForeColor = Color.White;
                    textBox1.BorderStyle = textBox2.BorderStyle = BorderStyle.FixedSingle;
                    this.BackColor = Color.FromArgb(46, 51, 73);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e) {
            string errore = verificaValido(textBox1.Text, textBox2.Text, testoText1, testoText2);
            if (errore != null) {
                MessageBox.Show(errore);
            } else {
                using (StreamWriter writeCliente = new StreamWriter(@"utente.cliente", true)) {
                    writeCliente.WriteLine(sha256(textBox1.Text));
                    writeCliente.WriteLine(sha256(textBox2.Text));
                }
                MessageBox.Show("Utente aggiunto con successo");
                this.Close();
            }
        }
        private void button4_Click(object sender, EventArgs e) {
            if (text2Testo)
                PremiBottoneHideView(textBox2, button4, button3);
        }
        private void button3_Click(object sender, EventArgs e) {
            if (text2Testo)
                PremiBottoneHideView(textBox2, button3, button4);
        }
        public static string sha256(string randomString) //FUNZIONE GENERA HASH CON SHA256 di Nicola
        {
            //MessageBox.Show(randomString);
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto) {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
        public static void PremiBottoneHideView(TextBox passwd, Button nascondi, Button vedi) {
            passwd.UseSystemPasswordChar = !passwd.UseSystemPasswordChar;
            nascondi.Hide();
            vedi.Show();
            passwd.Focus();
        }
        public static string verificaValido(string nomeUtente, string password, string text1Predef, string text2Predef) {
            string errore = null;

            if (nomeUtente == text1Predef || password == text2Predef) { //se sono uguali ai testi originali
                errore = "Immettere dei valori diversi da quelli di default";
                return errore;
            }

            if (String.IsNullOrWhiteSpace(nomeUtente) || String.IsNullOrWhiteSpace(password)) { //se sono vuoiti o hanno solo spazi bianchi
                errore = "Inserire valori validi in tutti i campi";
                return errore;
            }

            VerificaCheNonEsistaGiaUtente(@"utente.cliente", sha256(nomeUtente), "cliente", ref errore);
            VerificaCheNonEsistaGiaUtente(@"utente.proprietario", sha256(nomeUtente), "proprietario", ref errore);

            return errore;
        }
        public static void VerificaCheNonEsistaGiaUtente(string filename, string nomUt, string clientOprop, ref string err) {
            string rigaLetta;
            var J = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader readCliente = new StreamReader(J)) {
                do {
                    rigaLetta = readCliente.ReadLine();

                    if (rigaLetta == nomUt)
                        err = $"Username {clientOprop} già esistente. Usare un altro username";//la variabile clienteOprop mi indica se l'username che esiste già è di un cliente o di un proprietario

                    readCliente.ReadLine();//vado a capo perchè non mi interessa la password
                } while (rigaLetta != null && err == null);
            }
            J.Close();
        }
        private void button2_Click(object sender, EventArgs e) {
            this.Close();
        }
        private void textBox1_MouseClick(object sender, MouseEventArgs e) {
            if (!text1Testo)
                textBox1.Text = ""; //metto vero solo quando ci ho scritto testo sennò ogni volta che premo cancella
        }
        private void textBox1_TextChanged(object sender, EventArgs e) {
            text1Testo = true;
        }
        private void textBox2_MouseClick(object sender, MouseEventArgs e) {
            if (!text2Testo)
                textBox2.Text = "";
        }
        private void textBox2_TextChanged(object sender, EventArgs e) {
            text2Testo = true;
            if (volte == 2)
                textBox2.UseSystemPasswordChar = true;
            volte++;
        }
    }
}
