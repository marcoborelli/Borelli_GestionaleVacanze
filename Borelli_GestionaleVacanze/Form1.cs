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
using System.Security.Cryptography;

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
        string filenameSettings = @"impostasiu.ristorante", filenamePiatti = @"piatti.ristorante", filenameCheck = @"checksum.ristorante", filenameListaCSV = @"ordiniLista.csv";
        int volte = 0;
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
            volte++;
            CreaFileSeNonCe(filenameCheck);
            CreaFileSeNonCe(filenamePiatti);
            CreaFileSeNonCe(@"utente.proprietario");
            CreaFileSeNonCe(@"utente.cliente");

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
                button1.BackColor = button2.BackColor = button3.BackColor = button4.BackColor = textBox1.BackColor = textBox2.BackColor = Color.FromArgb(37, 42, 64);
                textBox1.BorderStyle = textBox2.BorderStyle = BorderStyle.FixedSingle;
                button1.ForeColor = button2.ForeColor = button3.ForeColor = button4.ForeColor = textBox1.ForeColor = textBox2.ForeColor = Color.White;
                this.BackColor = Color.FromArgb(46, 51, 73);
            }
            else
            {
                button1.BackColor = button2.BackColor = button3.BackColor = button4.BackColor = textBox1.BackColor = textBox2.BackColor = Color.White;
                textBox1.BorderStyle = textBox2.BorderStyle = BorderStyle.Fixed3D;
                button1.ForeColor = button2.ForeColor = button3.ForeColor = button4.ForeColor = textBox1.ForeColor = textBox2.ForeColor = Color.Black;
                this.BackColor = Form1.DefaultBackColor;
            }
        }
        private void button1_Click(object sender, EventArgs e) //nuova utenza
        {
            Form2 registrazione = new Form2();
            registrazione.Show();
        }
        private void button3_Click(object sender, EventArgs e)//vedi/no la password
        {
            if (text2Testo)
                PremiBottoneHideView(textBox2, button3, button4);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (text2Testo)
                PremiBottoneHideView(textBox2, button4, button3);
        }
        void prova_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = true;
            login = -1;
            text1Testo = text2Testo = false;
            textBox1.Text = testoText1;
            textBox2.Text = testoText2;
            textBox2.UseSystemPasswordChar = false;
            volte = 1;
            Form1_Load(sender, e);
        }
        private void button2_Click(object sender, EventArgs e) //login
        {
            Form3 prova = new Form3();
            prova.FormClosed += new FormClosedEventHandler(prova_FormClosed);
            string err = null;

            VerificaNumRigheFileCredenziali(@"utente.proprietario", "proprietario", ref err);
            VerificaNumRigheFileCredenziali(@"utente.cliente", "cliente", ref err);

            if (err != null)
            {
                MessageBox.Show(err);
                this.Close();
            }
            string rigaLetta;

            nomeUtente = sha256(textBox1.Text);
            passwd = sha256(textBox2.Text);

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
                if (FileChecksum(filenameCheck, filenamePiatti))
                    CreaForm(prova, this, "PROPRIETARIO", true);
                else
                {
                    DialogResult dialog = MessageBox.Show($"Il file non è stato modificato dal programma. Potrebbe avere errori. Continuare?\nNel caso in cui il file sia rovinato dovrebbero essere presenti dei backup nella cartella '\\backup'. Aprire lo zip più recente e ripristinare i file.", "ERRORE FILE", MessageBoxButtons.YesNo);

                    if (dialog == DialogResult.Yes)
                        CreaForm(prova, this, "PROPRIETARIO", true);
                    else
                        this.Close();
                }
            }
            else if (login == 2)
            {
                if (FileChecksum(filenameCheck, filenamePiatti))
                    CreaForm(prova, this, "CLIENTE", false);
                else
                {
                    DialogResult dialog = MessageBox.Show($"Il file non è stato modificato dal programma. Potrebbe avere errori. Continuare?\nNel caso in cui il file sia rovinato dovrebbero essere presenti dei backup nella cartella '\\backup'. Aprire lo zip più recente e ripristinare i file.", "ERRORE FILE", MessageBoxButtons.YesNo);

                    if (dialog == DialogResult.Yes)
                        CreaForm(prova, this, "CLIENTE", false);
                    else
                        this.Close();
                }
            }
            else if (login == -1)
                MessageBox.Show("Nome utente o password errati");

        }
        public static string sha256(string randomString) //FUNZIONE GENERA HASH CON SHA256 di Nicola
        {
            //MessageBox.Show(randomString);
            var crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
        public static void PremiBottoneHideView(TextBox passwd, Button nascondi, Button vedi)
        {
            passwd.UseSystemPasswordChar = !passwd.UseSystemPasswordChar;
            nascondi.Hide();
            vedi.Show();
            passwd.Focus();
        }
        public static void CreaFileSeNonCe(string filename)
        {
            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            p.Close();
        }
        public static void CreaForm(Form3 formella, Form1 u, string titolo, bool clienProp)
        {
            formella.ClienteProprietario = clienProp;//true=proprietario
            formella.Text = titolo;
            formella.Show();
            u.Hide();
        }
        public static bool FileChecksum(string filenameCheck, string filename)
        {
            string checksum;
            using (StreamReader checksumRead = new StreamReader(filenameCheck, false))
            {
                checksum = checksumRead.ReadLine();
                if (checksum == null && new FileInfo(filename).Length == 0)
                    checksum = GetMD5Checksum(filename);
            }

            if (checksum == GetMD5Checksum(filename))
                return true;
            else
                return false;
        }
        public static string GetMD5Checksum(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }
        public static void VerificaNumRigheFileCredenziali(string filenamee, string clientProp, ref string helo)
        {
            int nRighe = 0;
            using (StreamReader readProp = new StreamReader(filenamee, true))
            {
                while (readProp.ReadLine() != null)
                    nRighe++;
            }

            if (nRighe % 2 != 0)
                helo = $"C'è un errore nel file password del {clientProp}. Risolvere";

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
            if (volte == 2)
                textBox2.UseSystemPasswordChar = true;
            volte++;
        }
    }
}
