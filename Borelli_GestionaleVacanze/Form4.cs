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
        Encoding encoding = Encoding.GetEncoding(1252);
        public struct dimensioniRecord
        {
            public int padEliminato;
            public int padNome;
            public int padPrezzo;
            public int padIngredienti;
            public int padPosizione;
        }
        public struct piatto
        {
            public bool eliminato;
            public string nome;
            public double prezzo;
            public string[] ingredienti;
            public int posizione;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Enter)) //tasto salva
                button1.PerformClick();
            else if ((keyData == (Keys.NumPad1 | Keys.Control) || keyData == (Keys.D1 | Keys.Control)) && ClienteProprietario)//antipasto
                checkBox1.Checked = true;
            else if ((keyData == (Keys.NumPad2 | Keys.Control) || keyData == (Keys.D2 | Keys.Control)) && ClienteProprietario)//primo
                checkBox2.Checked = true;
            else if ((keyData == (Keys.NumPad3 | Keys.Control) || keyData == (Keys.D3 | Keys.Control)) && ClienteProprietario)//secondo
                checkBox3.Checked = true;
            else if ((keyData == (Keys.NumPad4 | Keys.Control) || keyData == (Keys.D4 | Keys.Control)) && ClienteProprietario)//dolce
                checkBox4.Checked = true;
            else if (keyData == (Keys.Add) && !ClienteProprietario && int.Parse(comboBox1.Text) < 10)
                comboBox1.Text = $"{int.Parse(comboBox1.Text) + 1}";
            else if (keyData == (Keys.Subtract) && !ClienteProprietario && int.Parse(comboBox1.Text) > 0)
                comboBox1.Text = $"{int.Parse(comboBox1.Text) - 1}";

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public int posizione { get; set; } //posizione del puntatore
        public bool modificaAggiungi { get; set; } //passo da 3, se è true sto modifcando 
        public bool giaEliminato { get; set; } //me lo passo da 3 e mi serve per capire se sto modificando un piatto eliminato o esistente 

        public bool ClienteProprietario { get; set; }//bool true=sei il proprietario false=sei il cliente

        public string NumeroOrdinazioni { get; set; }//me la passo dalla 3 e indica il numero di ordinazioni di un piatto che ci sono già
        public int nuovoNumOrdinazioni { get; set; }//lo passo dalla 4 alla 3 e indica il nuovo numero ordinazioni
        public bool CambiatoNumOrdinazioni { get; set; }//lo passo alla 3 e indica se ho cambiato numero ordinazioni
        public string nomeClienteTemp { get; set; }//me lo passo per poi ripassarlo alla 3

        string filename = @"piatti.ristorante", filenameSettings = @"impostasiu.ristorante";
        int record = 128;
        bool cambiato = false, ripassaPerForm4Load = true;

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            if (ripassaPerForm4Load)
            {
                using (StreamReader impostasiùRead = new StreamReader(filenameSettings, false))
                {
                    if (bool.Parse(impostasiùRead.ReadLine()))
                    {
                        button1.BackColor = textBox1.BackColor = textBox2.BackColor = textBox3.BackColor = textBox4.BackColor = textBox5.BackColor = textBox6.BackColor = comboBox1.BackColor = Color.FromArgb(37, 42, 64);
                        button1.ForeColor = textBox1.ForeColor = textBox2.ForeColor = textBox3.ForeColor = textBox4.ForeColor = textBox5.ForeColor = textBox6.ForeColor = comboBox1.ForeColor = checkBox1.ForeColor = checkBox2.ForeColor = checkBox3.ForeColor = checkBox4.ForeColor = label1.ForeColor = label2.ForeColor = label3.ForeColor = label4.ForeColor/* =*/ = Color.White;
                        textBox1.BorderStyle = textBox2.BorderStyle = textBox3.BorderStyle = textBox4.BorderStyle = textBox5.BorderStyle = textBox6.BorderStyle = BorderStyle.FixedSingle;
                        this.BackColor = Color.FromArgb(46, 51, 73);
                    }
                    else
                    {
                        button1.BackColor = textBox1.BackColor = textBox2.BackColor = textBox3.BackColor = textBox4.BackColor = textBox5.BackColor = textBox6.BackColor = comboBox1.BackColor = Color.White;
                        button1.ForeColor = textBox1.ForeColor = textBox2.ForeColor = textBox3.ForeColor = textBox4.ForeColor = textBox5.ForeColor = textBox6.ForeColor = comboBox1.ForeColor = checkBox1.ForeColor = checkBox2.ForeColor = checkBox3.ForeColor = checkBox4.ForeColor = label1.ForeColor = label2.ForeColor = label3.ForeColor = label4.ForeColor = Color.Black;
                        textBox1.BorderStyle = textBox2.BorderStyle = textBox3.BorderStyle = textBox4.BorderStyle = textBox5.BorderStyle = textBox6.BorderStyle = BorderStyle.Fixed3D;
                        this.BackColor = Form4.DefaultBackColor;
                    }
                }

                CambiatoNumOrdinazioni = false; //lo inizializzo false e mi indica se ho cambiato la text box con il numero di piatti della stessa portata

                dimensioniRecord campiRecord;
                piatto piattino;

                campiRecord.padEliminato = 5;
                campiRecord.padNome = 15;
                campiRecord.padPrezzo = 10;
                campiRecord.padIngredienti = 20;
                campiRecord.padPosizione = 1;

                if (!ClienteProprietario)//se cliente
                {
                    comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                    textBox1.Enabled = textBox2.Enabled = textBox3.Enabled = textBox4.Enabled = textBox5.Enabled = textBox6.Enabled = checkBox1.Enabled = checkBox2.Enabled = checkBox3.Enabled = checkBox4.Enabled = false;
                    button1.Text = "ESCI";

                    try
                    {
                        comboBox1.Text = $"{int.Parse(NumeroOrdinazioni)}";
                    }
                    catch
                    {
                        comboBox1.Text = "0";
                    }
                }
                else
                    comboBox1.Visible = label4.Visible = false;//se sono proprietario nasconto coso per selezionare quantità e rispettiva label

                string[] fields;
                string[] ingredienti;
                if (modificaAggiungi)//true=modifica false=aggiungi
                {
                    string riga;

                    var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    p.Seek(posizione, SeekOrigin.Begin);
                    using (BinaryReader reader = new BinaryReader(p, encoding))
                    {
                        riga = reader.ReadString();
                        fields = riga.Split(';');
                    }
                    p.Close();

                    ingredienti = fields[3].Split(',');

                    piattino = AssegnaAStruct(fields, ingredienti);
                    InserisciInBox(piattino, textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, checkBox1, checkBox2, checkBox3, checkBox4);
                }
                else//ormai resetto perchè nascondo la scheda e non la chiudo più
                {
                    textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = textBox5.Text = textBox6.Text = "";
                    checkBox1.Checked = checkBox2.Checked = checkBox3.Checked = checkBox4.Checked = false;
                    textBox1.Focus();
                }
            }
            ripassaPerForm4Load = false;
        }
        private void button1_Click(object sender, EventArgs e)//salva
        {
            if (ClienteProprietario)
            {
                piatto piattino;
                dimensioniRecord campiRecord;

                campiRecord.padEliminato = 5;
                campiRecord.padNome = 15;
                campiRecord.padPrezzo = 10;
                campiRecord.padIngredienti = 20;
                campiRecord.padPosizione = 1;

                textBox2.Text = textBox2.Text.Replace(".", ",");//così mi accetta anche la virgola nel double

                string error = CampiValidi(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text, checkBox1, checkBox2, checkBox3, checkBox4, campiRecord);

                if (error == null)
                {
                    int posizionee = NumDaCheckBox(checkBox1, checkBox2, checkBox3, checkBox4);//ottengo il numero da mettere come ultimo parametro
                    piattino = InserireInStructValori(campiRecord, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text, textBox6.Text, posizionee, giaEliminato);
                    ScriviFile(piattino, campiRecord, record, posizione, filename, encoding);//record=lungh. record; posizione= pos. puntatore già sulla riga giusta; modificaAggiungi è il bool della form 3; nummm è il numero scritto sul file
                    ripassaPerForm4Load = true;//così quando riapro mi rifà sta funziono solo una volta
                    this.Hide();
                }
                else
                    MessageBox.Show(error);
            }
            else
            {
                if (cambiato)
                {
                    nuovoNumOrdinazioni = (int.Parse(comboBox1.Text));
                    CambiatoNumOrdinazioni = true;//mi serve per la form 3
                    cambiato = false;
                }

                ripassaPerForm4Load = true;
                this.Hide();
            }

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Text = "SALVA ED ESCI";
            cambiato = true;
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            ripassaPerForm4Load = true;
            this.Hide();
        }
        private void Form4_Activated(object sender, EventArgs e)
        {
            Form4_Load(sender, e);//in questo modo anche se la nascondo faccio di modo che quando la riapro fosse come se la aprissi per la prima volta
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//antipasto
        {
            if (checkBox1.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox3, checkBox4);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)//primo
        {
            if (checkBox2.Checked)
                SelezionaSoloUnaCheckBox(checkBox1, checkBox3, checkBox4);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)//secondo
        {
            if (checkBox3.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox1, checkBox4);
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)//dolce
        {
            if (checkBox4.Checked)
                SelezionaSoloUnaCheckBox(checkBox2, checkBox3, checkBox1);
        }
        public static string ControlloPuntoEVirgola(string error, string campo)
        {
            for (int i = 0; i < campo.Length; i++)
            {
                if (campo.Substring(i, 1) == ";" || campo.Substring(i, 1) == ",")
                    return error = $"Il campo: '{campo}' non accetta il simboli ';' e ','";
            }
            return error; //così in ogni caso mettendo che il coso prima abbia dato errore me lo porto dietro e alla peggio lo sovrascrivo con un altro errore ma mai con un null
        }
        public static string CampiValidi(string nome, string prezzo, string ing1, string ing2, string ing3, string ing4, CheckBox uno, CheckBox due, CheckBox tre, CheckBox quattro, dimensioniRecord dimRecc)
        {
            string error = null;
            string[] campiGiustoPerFareCiclo = new string[] { nome, prezzo, ing1, ing2, ing3, ing4 };

            if (nome.Length > dimRecc.padNome || ing1.Length > dimRecc.padIngredienti || ing2.Length > dimRecc.padIngredienti || ing3.Length > dimRecc.padIngredienti || ing4.Length > dimRecc.padIngredienti)//controllo nome e ingredienti
                return error = "Inserire nei campi dei valori validi";

            if (!uno.Checked && !due.Checked && !tre.Checked && !quattro.Checked) //controllo che almeno uno sia selezionato
                return error = "Selezionare il tipo di portata";

            try//controllo prezzo
            {
                double helo = double.Parse(prezzo);
            }
            catch
            {
                return error = "Inserire un prezzo valido";
            }

            for (int i = 0; i < campiGiustoPerFareCiclo.Length; i++)//faccio un ciclo nel quale controlloogni campo he accetta stringhe che non contenga ; o ,
                error = ControlloPuntoEVirgola(error, campiGiustoPerFareCiclo[i]);

            return error;
        }
        public static void InserisciInBox(piatto piattuccio, TextBox nome, TextBox prezzo, TextBox ing1, TextBox ing2, TextBox ing3, TextBox ing4, CheckBox ant, CheckBox prim, CheckBox sec, CheckBox dol)
        {
            nome.Text = EliminaSpazi(piattuccio.nome);
            prezzo.Text = EliminaSpazi($"{piattuccio.prezzo}");
            ing1.Text = EliminaSpazi(piattuccio.ingredienti[0]);
            ing2.Text = EliminaSpazi(piattuccio.ingredienti[1]);
            ing3.Text = EliminaSpazi(piattuccio.ingredienti[2]);
            ing4.Text = EliminaSpazi(piattuccio.ingredienti[3]);

            if (piattuccio.posizione == 0)
                ant.Checked = true;
            else if (piattuccio.posizione == 1)
                prim.Checked = true;
            else if (piattuccio.posizione == 2)
                sec.Checked = true;
            else if (piattuccio.posizione == 3)
                dol.Checked = true;
        }
        public static void ScriviFile(piatto piatt, dimensioniRecord dimm, int record, int pos, string filename, Encoding encoding)
        {
            string ingr = "";

            for (int i = 0; i < piatt.ingredienti.Length; i++)
                ingr += $"{piatt.ingredienti[i]},";
            ingr = ingr.Substring(0, ingr.Length - 1);//tolgo la virgola finale

            string tot = $"{$"{piatt.eliminato}".PadRight(dimm.padEliminato)};{piatt.nome};{$"{piatt.prezzo}".PadRight(dimm.padPrezzo)};{ingr};{$"{piatt.posizione}".PadRight(dimm.padPosizione)};".PadRight(record - 1);
            //non metto il toUpper ovunque perchè l'ho già messo prima dove potevo

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(pos, SeekOrigin.Begin);
            //Encoding cp437 = Encoding.GetEncoding(437);
            using (BinaryWriter writer = new BinaryWriter(f, encoding))
            {
                writer.Write(tot);
            }
            f.Close();
        }
        public static piatto InserireInStructValori(dimensioniRecord dim, string nome, string prezzo, string ing1, string ing2, string ing3, string ing4, int pos, bool giaEliminato)
        {
            piatto piattuccino;
            piattuccino.ingredienti = new string[4];

            if (!giaEliminato) //se è false è perchè di là sto modifcando/creando un piatto che già esiste
                piattuccino.eliminato = true;
            else
                piattuccino.eliminato = false;

            piattuccino.nome = nome.PadRight(dim.padNome).ToUpper();
            piattuccino.prezzo = double.Parse(prezzo);
            piattuccino.ingredienti[0] = ing1.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[1] = ing2.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[2] = ing3.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.ingredienti[3] = ing4.PadRight(dim.padIngredienti).ToUpper();
            piattuccino.posizione = pos;

            return piattuccino;
        }
        public static int NumDaCheckBox(CheckBox uno, CheckBox due, CheckBox tre, CheckBox quattro)
        {
            int pos = -1;

            if (uno.Checked)
                pos = 0;
            else if (due.Checked)
                pos = 1;
            else if (tre.Checked)
                pos = 2;
            else if (quattro.Checked)
                pos = 3;

            return pos;
        }
        public static string EliminaSpazi(string elemento)
        {
            int i = elemento.Length;
            bool spazio = true;
            while (i > 0 && spazio)
            {
                if (elemento.Substring(i - 1, 1) == " ")
                    elemento = elemento.Substring(0, elemento.Length - 1);
                else
                    spazio = false;
                i--;
            }
            return elemento;
        }
        public static piatto AssegnaAStruct(string[] campi, string[] ingredienti)
        {
            piatto piattuccio;
            piattuccio.ingredienti = new string[ingredienti.Length];

            piattuccio.eliminato = bool.Parse(campi[0]);
            piattuccio.nome = campi[1];
            piattuccio.prezzo = double.Parse(campi[2]);

            for (int i = 0; i < ingredienti.Length; i++)
                piattuccio.ingredienti[i] = ingredienti[i];

            piattuccio.posizione = int.Parse(campi[4]);

            return piattuccio;
        }
        public static void SelezionaSoloUnaCheckBox(CheckBox uno, CheckBox due, CheckBox tre)
        {
            uno.Checked = due.Checked = tre.Checked = false;
        }
    }
}
