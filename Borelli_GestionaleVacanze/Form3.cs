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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Delete) && !recuperaPiatti) //cancellazione logica se si è su lista principale
                button2.PerformClick();
            else if (keyData == (Keys.Delete | Keys.Shift) && recuperaPiatti) //cancellazione fisica in parte recupera/elimina
                button4.PerformClick();
            else if (keyData == (Keys.R) && recuperaPiatti) //recupero piatti
                button2.PerformClick();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        string[,] backup = new string[23, 3];
        int record = 128, numm = 0;
        string filename = @"piatti.ristorante";
        bool modifica = false, recuperaPiatti = false;
        bool CrescDecr1 = false, CrescDecr3 = false;
        int volte = 0,nColonna=3;
        public bool ClienteProprietario { get; set; }//true=proprietario
        public Form3()
        {
            InitializeComponent();
            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            listView1.Columns.Add("NOME", 125);
            listView1.Columns.Add("PREZZO", 50);
            listView1.Columns.Add("INGREDIENTI", 255);
            listView1.Columns.Add("POSIZIONE", 80);

            button1.Text = "CREA ORDINE";
            button4.Hide();

        }

        private void Form3_Load(object sender, EventArgs e)
        {
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

            if (!ClienteProprietario)
            {
                var columnToRemove = listView1.Columns["QTA"];
                listView1.Columns.Remove(columnToRemove);
                listView1.CheckBoxes = true;
            }


        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
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
            ModificaAggiungi.ClienteProprietario = ClienteProprietario;

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
            if (ClienteProprietario)
            {
                dimensioniRecord campiRecord;

                campiRecord.padEliminato = 5;
                campiRecord.padNome = 15;
                campiRecord.padPrezzo = 10;
                campiRecord.padIngredienti = 20;
                campiRecord.padPosizione = 1;

                if (listView1.SelectedItems.Count > 0)
                {
                    for (int i = 0; i < listView1.SelectedItems.Count; i++)
                    {
                        int inizioRecord = cercaPiatto(listView1.SelectedItems[i].Text, filename, encoding) - record;
                        eliminaOripristinaPiatti(inizioRecord, recuperaPiatti, filename, record, campiRecord, encoding);
                    }
                }
                Form3_Load(sender, e);
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e) //textBox ricerca
        {
            backup = new string[listView1.Items.Count, 3];//prima di cambiare faccio backup di come erano le cose
            BackupElementiSelezionatiEQta(backup, listView1);
            using (StreamWriter uu = new StreamWriter(@"heloo.txt"))
            {
                for (int i = 0; i < backup.GetLength(0); i++)
                    uu.WriteLine($"'{backup[i, 0]}'\t'{backup[i, 1]}''\t'{backup[i, 2]}");
            }

            listView1.Items.Clear();
            //0=stampa solo visibili 1=ricerca solo visibili 2=stampa solo eliminati 3= ricerca solo eliminati
            if ((textBox1.Text == "" || textBox1.Text == null) && !recuperaPiatti)
                StampaElementi(listView1, filename, 0, "", encoding);
            else if (!recuperaPiatti)
                StampaElementi(listView1, filename, 1, textBox1.Text.ToUpper(), encoding);
            else if ((textBox1.Text == "" || textBox1.Text == null) && recuperaPiatti)
                StampaElementi(listView1, filename, 2, "", encoding);
            else
                StampaElementi(listView1, filename, 3, textBox1.Text.ToUpper(), encoding);

            if (CrescDecr1) //così non mi sballa ordine quando lo riseleziono
                CrescDecr1 = false;
            else
                CrescDecr1 = true;

            if (CrescDecr3)
                CrescDecr3 = false;
            else
                CrescDecr3 = true;

            OrdinaElementi(nColonna, listView1, ref CrescDecr1, ref CrescDecr3); //li ordino per l'ultima categoria ordinata

            if (volte > 1)
                RipristinaIlBackup(backup, listView1);
            volte++;
        }
        private void button4_Click(object sender, EventArgs e)//elimina fisicamente
        {
            if (ClienteProprietario)
            {
                string heloo = "il piatto: ";
                string[] nomePiatto = new string[1];
                int y = listView1.SelectedItems.Count;
                bool selezione = false;
                if (y > 0)
                {
                    selezione = true;
                    nomePiatto = new string[y];
                    for (int i = 0; i < y; i++)
                    {
                        nomePiatto[i] = listView1.SelectedItems[i].Text;
                        heloo += $"{EliminaSpazi(nomePiatto[i])}, ";
                    }
                    heloo = heloo.Substring(0, heloo.Length - 2);
                }
                else
                    heloo = "tutti i piatti";

                DialogResult dialog = MessageBox.Show($"Così facendo perderai definitivamente {heloo}. Sicuro di volerlo fare?", "ELIMINAZIONE FISICA", MessageBoxButtons.YesNo);

                if (dialog == DialogResult.Yes)
                {
                    if (selezione)
                    {
                        for (int i = 0; i < y; i++)
                        {
                            EliminaDefinitivamente(filename, ref numm, record, nomePiatto[i], encoding);
                        }
                    }
                    else
                        EliminaDefinitivamente(filename, ref numm, record, null, encoding);
                }

                textBox1_TextChanged(sender, e);
            }
        }
        private void button3_Click(object sender, EventArgs e)//recupera piatti
        {
            if (ClienteProprietario)
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

        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            nColonna = e.Column;
            OrdinaElementi(nColonna, listView1, ref CrescDecr1, ref CrescDecr3);
            listView1.Refresh();
        }
        public static void RipristinaIlBackup(string[,] backup, ListView listino)
        {
            int ind;
            for (int i = 0; i < listino.Items.Count; i++)
            {
                ind = TrovaIndiceBackup(backup, listino.Items[i].SubItems[0].Text);
                listino.Items[i].Checked = bool.Parse(backup[ind, 1]);
                listino.Items[i].SubItems[4].Text = backup[ind, 2];
            }
        }
        public static int TrovaIndiceBackup(string[,] backup, string nome)
        {
            for (int i = 0; i < backup.GetLength(0); i++)
            {
                if (backup[i, 0] == nome)
                    return i;
            }
            return -1;
        }
        public static void BackupElementiSelezionatiEQta(string[,] backup, ListView listino)
        {
            for (int i = 0; i < listino.Items.Count; i++)
            {
                backup[i, 0] = listino.Items[i].SubItems[0].Text;
                if (listino.Items[i].Checked)
                    backup[i, 1] = "True";
                else
                    backup[i, 1] = "False";

                if (listino.Items[i].SubItems[4].Text != null && listino.Items[i].SubItems[4].Text != "")
                    backup[i, 2] = listino.Items[i].SubItems[4].Text;
                else
                    backup[i, 2] = null;
            }
        }
        public static void OrdinaElementi(int colonna, ListView listuccina, ref bool CrescDecr1, ref bool CrescDecr3)
        {
            if (colonna == 0)
            {
                if (listuccina.Sorting == SortOrder.None || listuccina.Sorting == SortOrder.Descending)
                    listuccina.Sorting = SortOrder.Ascending;
                else
                    listuccina.Sorting = SortOrder.Descending;
            }
            else if (colonna == 1)
            {
                if (CrescDecr1)
                {
                    for (int i = 0; i < listuccina.Items.Count; i++)
                    {
                        for (int j = i; j < listuccina.Items.Count; j++)
                        {
                            if (int.Parse(listuccina.Items[i].SubItems[1].Text) > int.Parse(listuccina.Items[j].SubItems[1].Text))
                                ScambiaElementi(i, j, listuccina);
                        }
                    }
                    CrescDecr1 = false;
                }
                else
                {
                    for (int i = 0; i < listuccina.Items.Count; i++)
                    {
                        for (int j = i; j < listuccina.Items.Count; j++)
                        {
                            if (int.Parse(listuccina.Items[i].SubItems[1].Text) < int.Parse(listuccina.Items[j].SubItems[1].Text))
                                ScambiaElementi(i, j, listuccina);
                        }
                    }
                    CrescDecr1 = true;
                }
            }
            else if (colonna == 3)
            {
                if (CrescDecr3)
                {
                    for (int i = 0; i < listuccina.Items.Count; i++)
                    {
                        for (int j = i; j < listuccina.Items.Count; j++)
                        {
                            if (RitornaIntPosizione(listuccina.Items[i].SubItems[3].Text) > RitornaIntPosizione(listuccina.Items[j].SubItems[3].Text))
                                ScambiaElementi(i, j, listuccina);
                        }
                    }
                    CrescDecr3 = false;
                }
                else
                {
                    for (int i = 0; i < listuccina.Items.Count; i++)
                    {
                        for (int j = i; j < listuccina.Items.Count; j++)
                        {
                            if (RitornaIntPosizione(listuccina.Items[i].SubItems[3].Text) < RitornaIntPosizione(listuccina.Items[j].SubItems[3].Text))
                                ScambiaElementi(i, j, listuccina);
                        }
                    }
                    CrescDecr3 = true;
                }

            }
        }
        public static int RitornaIntPosizione(string pos)
        {
            if (pos == "ANTIPASTO")
                return 0;
            else if (pos == "PRIMO")
                return 1;
            else if (pos == "SECONDO")
                return 2;
            else if (pos == "DOLCE")
                return 3;

            return -1;
        }
        public static void ScambiaElementi(int ind1, int ind2, ListView listuccia)
        {
            string[] backup = new string[] { " ", " ", " ", " " };
            string[] backup1 = new string[] { " ", " ", " ", " " };

            for (int i = 0; i < listuccia.Items[ind1].SubItems.Count - 1; i++)
                backup[i] = listuccia.Items[ind1].SubItems[i].Text;

            for (int i = 0; i < listuccia.Items[ind2].SubItems.Count - 1; i++)
                backup1[i] = listuccia.Items[ind2].SubItems[i].Text;


            for (int i = 0; i < listuccia.Items[ind2].SubItems.Count - 1; i++)
                listuccia.Items[ind2].SubItems[i].Text = backup[i];

            for (int i = 0; i < listuccia.Items[ind1].SubItems.Count - 1; i++)
                listuccia.Items[ind1].SubItems[i].Text = backup1[i];
        }
        public static void EliminaDefinitivamente(string filename, ref int numm, int record, string SoloUnoDaEliminare, Encoding encoding)
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
                if (indiciEliminati[i] != -1 && SoloUnoDaEliminare == null) //seconda condizione messa per fare il ciclo solo una volta in caso di selezione di un solo elemento
                    nVolte++;
            }

            for (int i = 0; i < nVolte; i++)
            {
                if (SoloUnoDaEliminare != null)
                    posPunt = IndiceUnicoDaEliminare;
                else
                {
                    trovaEliminati(indiciEliminati, filename, record, numm, false, ref IndiceUnicoDaEliminare, null, encoding);
                    posPunt = indiciEliminati[0];//prendo sempre il primo anche perchè gli indici cambiano
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
                        p.Close();

                } while (dentroTesto);//c'è while perchè porto su tutti quelli che stanno sotto

                var U = new FileStream(@"recordUsati.txt", FileMode.Create, FileAccess.ReadWrite);//diminuisco numero in file
                numm--;
                using (StreamWriter write = new StreamWriter(U))
                {
                    write.Write($"{numm}");
                }
                U.Close();
                //MessageBox.Show("FATTO");
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
