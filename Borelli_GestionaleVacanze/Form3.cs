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
using System.Text.RegularExpressions;

namespace Borelli_GestionaleVacanze
{
    public partial class Form3 : Form
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

        int record = 128, numm = 0;
        string filename = @"piatti.ristorante";
        bool modifica = false, recuperaPiatti = false;
        public Form3()
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            listView1.Columns.Add("NOME", 140);
            listView1.Columns.Add("PREZZO", 60);
            listView1.Columns.Add("INGREDIENTI", 300);
            listView1.Columns.Add("POSIZIONE", 80);

            button4.Hide();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            //StampaElementi(listView1, filename, false, "");
            textBox1_TextChanged(sender, e);

            var W = new FileStream(@"recordUsati.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using (StreamReader read = new StreamReader(W))
            {
                string num = read.ReadLine();
                if (num == null)
                {
                    using (StreamWriter write = new StreamWriter(W))
                    {
                        write.Write("0");
                    }
                }
                else
                {
                    try
                    {
                        numm = int.Parse(num);
                    }
                    catch
                    {
                        MessageBox.Show("Errore file 'RecordUsati.txt' il programma si chiuderà");
                        Application.Exit();
                    }
                }
            }
            W.Close();
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems != null)
            {
                //MessageBox.Show($"'{listView1.SelectedItems[0].Text}'");
                modifica = true;
                button1_Click(sender, e);
            }

        }
        private void button1_Click(object sender, EventArgs e) //aggiunta piatto
        {
            Form4 ModificaAggiungi = new Form4();
            ModificaAggiungi.giaEliminato = recuperaPiatti;

            if (modifica)
                ModificaAggiungi.posizione = cercaPiatto(listView1.SelectedItems[0].Text, filename, encoding) - record;//-record perchè lui mi da il numero quando ha finito dileggere riga quindi torno a inizio
            else
                ModificaAggiungi.posizione = record * numm;

            ModificaAggiungi.modificaAggiungi = modifica;//metto il bool nella form 4 uguale a questo bool

            modifica = false;
            //listBox1.ClearSelected();//deseleziono
            ModificaAggiungi.nummm = numm;

            ModificaAggiungi.Show();
        }
        private void Form3_Activated(object sender, EventArgs e)
        {
            Form3_Load(sender, e);
        }

        private void button2_Click(object sender, EventArgs e) //eliminazione logica piatto
        {
            dimensioniRecord campiRecord;

            campiRecord.padEliminato = 5;
            campiRecord.padNome = 15;
            campiRecord.padPrezzo = 10;
            campiRecord.padIngredienti = 20;
            campiRecord.padPosizione = 1;

            if (listView1.SelectedItems.Count > 0)
            {
                int inizioRecord = cercaPiatto(listView1.SelectedItems[0].Text, filename, encoding) - record;
                eliminaOripristinaPiatti(inizioRecord, recuperaPiatti, filename, record, campiRecord, encoding);

                Form3_Load(sender, e);
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e) //textBox ricerca
        {
            //0=stampa solo visibili 1=ricerca solo visibili 2=stampa solo eliminati 3= ricerca solo eliminati
            if ((textBox1.Text == "" || textBox1.Text == null) && !recuperaPiatti)
                StampaElementi(listView1, filename, 0, "", encoding);
            else if (!recuperaPiatti)
                StampaElementi(listView1, filename, 1, textBox1.Text.ToUpper(), encoding);
            else if ((textBox1.Text == "" || textBox1.Text == null) && recuperaPiatti)
                StampaElementi(listView1, filename, 2, "", encoding);
            else
                StampaElementi(listView1, filename, 3, textBox1.Text.ToUpper(), encoding);
        }
        private void button4_Click(object sender, EventArgs e)//elimina fisicamente
        {
            string heloo = "il piatto: ", nomePiatto=null;
            if (listView1.SelectedItems.Count>0)
            {
                nomePiatto = listView1.SelectedItems[0].Text;
                heloo += EliminaSpazi(nomePiatto);
            }
            else
                heloo = "tutti i piatti";

            DialogResult dialog = MessageBox.Show($"Così facendo perderai definitivamente {heloo}. Sicuro di volerlo fare?", "ELIMINAZIONE FISICA", MessageBoxButtons.YesNo);
            
            if (dialog == DialogResult.Yes)
                    EliminaDefinitivamente(filename, numm, record, nomePiatto, encoding);

            textBox1_TextChanged(sender, e);
        }
        private void button3_Click(object sender, EventArgs e)//recupera piatti
        {
            listView1.Items.Clear();
            if (!recuperaPiatti) //è false di default
            {
                button1.Enabled = false;
                button2.Text = "RIPRISTINA";
                button3.Text = "TORNA AI PIATTI ESISTENTI";
                recuperaPiatti = true;
                button4.Show();
                textBox1_TextChanged(sender, e);
            }
            else
            {
                button1.Enabled = true;
                button2.Text = "ELIMINA PIATTO ";
                button3.Text = "RECUPERA PIATTI";
                recuperaPiatti = false;
                button4.Hide();
                textBox1_TextChanged(sender, e);
            }

        }
        public static void EliminaDefinitivamente(string filename, int numm, int record, string SoloUnoDaEliminare, Encoding encoding)
        {
            int nVolte = 0, posPunt = 0, IndiceUnicoDaEliminare = 0;
            string rigaTemp = "";
            bool dentroTesto = false;

            int[] indiciEliminati = new int[numm];

            if (SoloUnoDaEliminare != null)//se ho selezionato un elemento
            {
                trovaEliminati(indiciEliminati, filename, record, numm, true, ref IndiceUnicoDaEliminare, SoloUnoDaEliminare, encoding);
                nVolte = 1; //così mi fa ciclo una sola volta
            }
            else
                trovaEliminati(indiciEliminati, filename, record, numm, false, ref IndiceUnicoDaEliminare, null, encoding);

            for (int i = 0; i < indiciEliminati.Length; i++) //trovo quante volte dovrò fare il ciclo per eliminare
            {
                if (indiciEliminati[i] != -1&& SoloUnoDaEliminare == null) //seconda condizione messa per fare il ciclo solo una volta in caso di selezione di un solo elemento
                    nVolte++;
            }

            for (int i = 0; i < nVolte; i++)
            {
                if (SoloUnoDaEliminare != null)
                    posPunt = IndiceUnicoDaEliminare;
                else
                {
                    trovaEliminati(indiciEliminati, filename, record, numm, false, ref IndiceUnicoDaEliminare, null, encoding);
                    posPunt = indiciEliminati[0];//prendo sempr eil primo anche perchè gli indici cambiano
                }


                do
                {
                    var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    dentroTesto = false;

                    if (posPunt + record < record * numm) //controllo di stare dentro testo
                    {
                        dentroTesto = true;
                        p.Seek(posPunt + record, SeekOrigin.Begin);//mi posiziono su riga sotto
                        using (BinaryReader reader = new BinaryReader(p, encoding))
                        {
                            rigaTemp = reader.ReadString();
                        }
                        p.Close();
                        var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        y.Seek(posPunt, SeekOrigin.Begin); //mi posiziono su robo da eliminare e lo sovrascrivo
                        using (BinaryWriter writer = new BinaryWriter(y, encoding))
                        {
                            writer.Write(rigaTemp);
                        }
                        y.Close();
                        posPunt += record;
                    }
                    else
                    {
                        p.Close();
                    }

                } while (dentroTesto);//c'è while perchè porto su tutti quelli che stanno sotto

                var U = new FileStream(@"recordUsati.txt", FileMode.Create, FileAccess.ReadWrite);//diminuisco numero in file
                numm--;
                using (StreamWriter write = new StreamWriter(U))
                {
                    write.Write($"{numm}");
                }
                U.Close();
            }


            var fileNuovo = new FileStream(@"menu.piattoTemp", FileMode.OpenOrCreate, FileAccess.ReadWrite); //copio solo fino a cose che esistono ancora su un file temporaneo
            var fileOriginale = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            BinaryReader vecchio = new BinaryReader(fileOriginale, encoding);
            BinaryWriter nuovo = new BinaryWriter(fileNuovo, encoding);

            fileOriginale.Seek(0, SeekOrigin.Begin);

            while (fileOriginale.Position < record * numm)
            {
                string temp;
                temp = vecchio.ReadString();
                nuovo.Write(temp);
            }

            fileNuovo.Close();
            fileOriginale.Close();


            SovrascrivereFile(@"menu.piattoTemp", filename);
        }
        public static void SovrascrivereFile(string fileTemp, string fileOrig)
        {
            FileInfo fi = new FileInfo(fileTemp);
            FileInfo newFi = new FileInfo(fileOrig);
            newFi.Delete();
            newFi = fi.CopyTo(fileOrig);
            fi.Delete();
        }
        public static void trovaEliminati(int[] indici, string filename, int record, int cosiUsati, bool soloUno, ref int indiceSoloUno, string piattoSoloUno, Encoding encoding)
        {
            string riga = "";
            string[] fields;
            int indUsati = 0;
            for (int i = 0; i < indici.Length; i++)
                indici[i] = -1;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            while (p.Position < record * cosiUsati)
            {
                riga = reader.ReadString();
                fields = riga.Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (!bool.Parse(fields[0]))
                {
                    if (soloUno && fields[1] == piattoSoloUno)//se voglio eliminare fisicamente un solo piatto
                    {
                        indiceSoloUno = Convert.ToInt32(p.Position) - record;
                        p.Position = record * cosiUsati; //così esco da ciclo
                    }
                    else if (!soloUno)
                    {
                        indici[indUsati] = Convert.ToInt32(p.Position) - record;
                        indUsati++;
                    }
                }
            }
            p.Close();
        }
        public static void eliminaOripristinaPiatti(int inizioRecord, bool eliminaRipristina, string filename, int record, dimensioniRecord lunghRec, Encoding encoding)
        {
            string[] fields;
            string riga;
            bool eliminato;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            p.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryReader reader = new BinaryReader(p, encoding))
            {
                riga = reader.ReadString();
                fields = riga.Split(';');
            }
            p.Close();

            if (eliminaRipristina)//se è true è perchè sto recuperando un piatto
                eliminato = true;
            else
                eliminato = false;

            var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            y.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(y, encoding))
            {
                string totale = $"{$"{eliminato}".PadRight(lunghRec.padEliminato)};{fields[1]};{fields[2]};{fields[3]};{fields[4]};".PadRight(record - 1);
                //Console.WriteLine($"NEW: '{totale}' {totale.Length}\nOLD: '{riga}' {riga.Length}\nRECORD: {record}");
                //Console.ReadKey();
                writer.Write(totale);
            }
            y.Close();
        }
        public static void StampaElementi(ListView listino, string filename, int ricerca, string testoRicerca, Encoding encoding)
        {
            listino.Items.Clear();
            string riga;

            string[] fields;
            string[] fieldsRidotti;
            string[] ingredienti;

            var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            f.Seek(0, SeekOrigin.Begin);
            using (BinaryReader leggiNomi = new BinaryReader(f, encoding))
            {
                while (f.Position < f.Length)
                {
                    riga = leggiNomi.ReadString();
                    fields = riga.Split(';');

                    fieldsRidotti = new string[fields.Length - 1]; //così non stampo a video il bool dell'eleminazione
                    for (int i = 0; i < fields.Length - 1; i++)
                        fieldsRidotti[i] = fields[i + 1];

                    ingredienti = fieldsRidotti[2].Split(','); //per togliere spazi in eccesso
                    fieldsRidotti[2] = "";
                    for (int i = 0; i < ingredienti.Length; i++)
                    {
                        ingredienti[i] = $"{EliminaSpazi(ingredienti[i])}";
                        fieldsRidotti[2] += $"{ingredienti[i]}, ";
                    }
                    fieldsRidotti[2] = fieldsRidotti[2].Substring(0, fieldsRidotti[2].Length - 2);

                    if (int.Parse(fieldsRidotti[3]) == 0)
                        fieldsRidotti[3] = "ANTIPASTO";
                    else if (int.Parse(fieldsRidotti[3]) == 1)
                        fieldsRidotti[3] = "PRIMO";
                    else if (int.Parse(fieldsRidotti[3]) == 2)
                        fieldsRidotti[3] = "SECONDO";
                    else if (int.Parse(fieldsRidotti[3]) == 3)
                        fieldsRidotti[3] = "DOLCE";

                    Regex rx = new Regex(testoRicerca);

                    if ((ricerca == 0 && bool.Parse(fields[0])) || //se il coso non è eliminato
                        (ricerca == 1 && rx.IsMatch(fieldsRidotti[0]) && bool.Parse(fields[0])) || //se non è eliminato e corrisponde a ricerca
                        (ricerca == 2 && !bool.Parse(fields[0])) || //se è eliminato
                        (ricerca == 3 && rx.IsMatch(fieldsRidotti[0]) && !bool.Parse(fields[0]))) //se è eliminato e corrisponde a ricerca
                    {
                        ListViewItem item = new ListViewItem(fieldsRidotti);
                        listino.Items.Add(item);
                    }
                }
            }
            f.Close();

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

        public static int cercaPiatto(string nomePiatto, string filename, Encoding encoding)
        {
            int pos = -1;
            bool corrispondenza = true;
            string riga = "";
            string[] fields;
            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            p.Seek(0, SeekOrigin.Begin);

            while (p.Position < p.Length && corrispondenza)
            {
                riga = reader.ReadString();
                fields = riga.Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (fields[1] == nomePiatto)
                {
                    corrispondenza = true;
                    pos = Convert.ToInt32(p.Position);
                }
            }
            p.Close();

            return pos;
        }
    }
}
