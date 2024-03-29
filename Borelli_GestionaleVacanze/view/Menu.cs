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

namespace Borelli_GestionaleVacanze {
    public partial class Menu : Form {
        InfoPiatto ModificaAggiungi = new InfoPiatto();
        Settings Impostasiu = new Settings();
        Encoding encoding = Encoding.GetEncoding(1252);

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Delete) && !recuperaPiatti && ClienteProprietario) //cancellazione logica se si è su lista principale
                button2.PerformClick();
            else if (keyData == (Keys.Delete | Keys.Shift) && recuperaPiatti && ClienteProprietario) //cancellazione fisica in parte recupera/elimina
                button4.PerformClick();
            else if (keyData == (Keys.R | Keys.Shift) && recuperaPiatti && ClienteProprietario) //recupero piatti
                button2.PerformClick();
            else if (keyData == (Keys.N | Keys.Shift) && !recuperaPiatti && ClienteProprietario)
                button1.PerformClick();

            return base.ProcessCmdKey(ref msg, keyData);
        }

        string[,] backup;// = new string[1, 2];
        int numm = 0;
        string filename = @"piatti.ristorante", filenameSettings = @"impostasiu.ristorante", filenameCheck = @"checksum.ristorante";
        bool modifica = false, recuperaPiatti = false; //modifca= mi permette di caricare o no su form 4 eventuali dati. RecuperaPiatti si attiva quando si preme bottone per recuperare/eliminare
        bool CrescDecr1 = false, CrescDecr3 = false;
        bool giaPremutoCreaListaCliente = false; //serve per eventuali conflitti tra dark mode e la lista totale del cliente
        int volte = 0, nColonna = 3;//nColonna l'ho messa così poi per riordinare non riordino sempre per antipasti ma per ultima categoria scelta; è uguale a 3 perchè all'inizio ordino per portata
        double totCliente = 0;
        bool darkmode = false, bohLogout = true, salvaOrdineSuFile = false;

        public bool ClienteProprietario { get; set; }//true=proprietario
        public Menu() {
            InitializeComponent();
            listView1.View = listaSCONTRINO.View = View.Details;
            listView1.FullRowSelect = listaSCONTRINO.FullRowSelect = true;

            string[] nomeColonne1 = new string[] { "NOME", "PREZZO", "INGREDIENTI", "POSIZIONE", "QTA" };
            int[] dimColonne1 = new int[] { 125, 50, 255, 80, 50 };
            string[] nomeColonne2 = new string[] { "NOME", "QTA", "PREZZO" };
            int[] dimColonne2 = new int[] { 80, 50, 50 };

            for (int i=0; i<nomeColonne1.Length; i++) {
                listView1.Columns.Add(nomeColonne1[i], dimColonne1[i]);
            }
            for (int i = 0; i < nomeColonne2.Length; i++) {
                listaSCONTRINO.Columns.Add(nomeColonne2[i], dimColonne2[i]);
            }

            button4.Hide();

            DimensioniRecord.PadEliminato = 5;
            DimensioniRecord.PadNome = 15;
            DimensioniRecord.PadPrezzo = 10;
            DimensioniRecord.PadIngredienti = 20;
            DimensioniRecord.PadPosizione = 1;
            DimensioniRecord.Record = 128;
        }
        private void Form3_Load(object sender, EventArgs e) {
            if (!ClienteProprietario)//se cliente
            {
                if (!giaPremutoCreaListaCliente) {//visto che con la dark mode quando la cambio torno qui potrei essere dentro un ordine ma lui mi resetterebbe i bottoni e i testi{
                    button5.Enabled = false;
                    button2.Text = "CREA ORDINE";
                } else {
                    //button5.Enabled = true;
                    button2.Text = "MODIFICA ORDINE";
                }

                button2.Location = new System.Drawing.Point(649, 10);//lo metto dove stava l'1
                button1.Visible = button3.Visible = false;
            } else if (volte < 1) {//solo la prima volta la tolgo e se sono proprietario
                listView1.Columns.Remove(listView1.Columns[4]);
                listaSCONTRINO.Visible = button5.Visible = false;
            }

            if (ModificaAggiungi.CambiatoNumOrdinazioni)//se sono cliente e ho modificato numero ordinazioni
            {
                int AAAAA = TrovaIndiceDentroListView(ModificaAggiungi.nomeClienteTemp, listView1);//trovo l'indice di quello che avevo selezionato prima
                listView1.Items[AAAAA].SubItems[4].Text = $"{ModificaAggiungi.nuovoNumOrdinazioni}";
                AggiornaBackup(backup, ModificaAggiungi.nomeClienteTemp, $"{ModificaAggiungi.nuovoNumOrdinazioni}");
            }

            ModificaAggiungi.CambiatoNumOrdinazioni = false;//così la prossima volta non fa più

            using (StreamReader impostasiùRead = new StreamReader(filenameSettings, false))//parte dark mode
            {
                try {
                    darkmode = bool.Parse(impostasiùRead.ReadLine());
                    Color backElem, fore, backForm;
                    if (darkmode) {
                        backElem = Color.FromArgb(37, 42, 64);
                        fore = Color.White;
                        backForm = Color.FromArgb(46, 51, 73);
                    } else {
                        backElem = Color.White;
                        fore = Color.Black;
                        backForm = Borelli_GestionaleVacanze.Menu.DefaultBackColor;
                    }

                    button1.BackColor = button2.BackColor = button3.BackColor = button4.BackColor = button5.BackColor = button6.BackColor = button7.BackColor = listView1.BackColor = listaSCONTRINO.BackColor = textBox1.BackColor = backElem;
                    button1.ForeColor = button2.ForeColor = button3.ForeColor = button4.ForeColor = button5.ForeColor = button6.ForeColor = button7.ForeColor = label1.ForeColor = listView1.ForeColor = listaSCONTRINO.ForeColor = textBox1.ForeColor = fore;
                    this.BackColor = backForm;

                    if (darkmode && giaPremutoCreaListaCliente)//perchè da disabilitata diventa bianca e bianco su bianco non si vede
                        listView1.ForeColor = Color.Black;

                    salvaOrdineSuFile = bool.Parse(impostasiùRead.ReadLine());
                } catch {
                    MessageBox.Show("File impostazioni corrotto. Il programma si chiuderà");
                    Environment.Exit(1);
                }

            }

            textBox1_TextChanged(sender, e);
        }
        private void Form3_FormClosing(object sender, FormClosingEventArgs e) {
            ScriviFileChecksum(filenameCheck, filename);
            if (bohLogout)
                Environment.Exit(1);
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (listView1.SelectedItems.Count > 0) {
                modifica = true;
                button1_Click(sender, e);
            }
        }
        private void button1_Click(object sender, EventArgs e) //aggiunta piatto
        {
            ModificaAggiungi.giaEliminato = recuperaPiatti; //è la variabile che cambio quando premo il bottone di recupero piatti
            ModificaAggiungi.ClienteProprietario = ClienteProprietario;//true=proprietario
            ModificaAggiungi.modificaAggiungi = modifica;

            if (modifica) { // se modifica è true è perchè ho fatto doppio click su elemento
                ModificaAggiungi.posizione = cercaPiatto(listView1.SelectedItems[0].Text, filename, encoding) - DimensioniRecord.Record;//-record perchè lui mi da il numero quando ha finito di leggere riga quindi torno a inizio
            } else {
                ModificaAggiungi.posizione = DimensioniRecord.Record * numm;
            }

            if (!ClienteProprietario) { //se è cliente lo vede
                ModificaAggiungi.NumeroOrdinazioni = listView1.SelectedItems[0].SubItems[4].Text;
                ModificaAggiungi.nomeClienteTemp = listView1.SelectedItems[0].Text;//il nome del piatto
            }

            modifica = false;//resetto variabile che mi faceva capiure se venivo da doppio click 

            ModificaAggiungi.Show();
        }
        private void Form3_Activated(object sender, EventArgs e) {
            Form3_Load(sender, e);
        }

        private void button2_Click(object sender, EventArgs e) //eliminazione logica piatto
        {
            if (ClienteProprietario) {
                if (listView1.SelectedItems.Count > 0) {
                    for (int i = 0; i < listView1.SelectedItems.Count; i++) {
                        int inizioRecord = cercaPiatto(listView1.SelectedItems[i].Text, filename, encoding) - DimensioniRecord.Record;
                        eliminaOripristinaPiatti(inizioRecord, recuperaPiatti, filename, encoding);
                    }
                }
                Form3_Load(sender, e);
            } else//l'ho messo nel bottone 2 e non nell'1 perchè nell'1 ci sono già funzioni riguardo la parte del cliente (doppio click su elemento) quindi separo le cose
              {
                bool controllino = false;
                for (int i = 0; i < backup.GetLength(0); i++) {
                    if (backup[i, 3] == "Color.Yellow")
                        controllino = true;
                }

                if (controllino) {
                    if (!giaPremutoCreaListaCliente)//così una volta disabilito list view e la seconda la riabilito
                    {
                        button2.Text = "MODIFICA ORDINE";
                        button5.Enabled = true;
                        listView1.Enabled = false;

                        if (darkmode)
                            listView1.ForeColor = Color.Black;

                        totCliente = 0;
                        for (int i = 0; i < backup.GetLength(0); i++) {
                            if (backup[i, 3] == "Color.Yellow")//se è un'ordine
                            {
                                string[] temp = new string[backup.GetLength(1) - 1];//-1 perchè tanto non mi importa dell'ultimo campo (il colore)

                                for (int j = 0; j < temp.Length; j++) {
                                    if (j == temp.Length - 1)//se è il prezzo lo moltiplico per la quantità
                                    {
                                        temp[j] = $"{double.Parse(backup[i, j - 1]) * double.Parse(backup[i, j])} $";
                                        totCliente += double.Parse(backup[i, j - 1]) * double.Parse(backup[i, j]);
                                    } else
                                        temp[j] = backup[i, j];
                                }

                                ListViewItem item = new ListViewItem(temp);
                                listaSCONTRINO.Items.Add(item);
                            }
                        }
                        button5.Text = $"OK. TOT: {totCliente}$";
                        giaPremutoCreaListaCliente = true;
                    } else {
                        listView1.Enabled = true;
                        button5.Enabled = false;
                        giaPremutoCreaListaCliente = false;
                        button2.Text = "CREA ORDINE";
                        listaSCONTRINO.Items.Clear();
                    }
                } else
                    MessageBox.Show("Prima selezione dei piatti");
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e) //textBox ricerca
        {
            listView1.Items.Clear();

            if (textBox1.Text == String.Empty && !recuperaPiatti) { //0=stampa solo visibili 1=ricerca solo visibili 2=stampa solo eliminati 3= ricerca solo eliminati
                StampaElementi(listView1, filename, 0, "", ref numm, encoding);
            } else if (!recuperaPiatti) {
                StampaElementi(listView1, filename, 1, textBox1.Text.ToUpper(), ref numm, encoding);
            } else if (textBox1.Text == String.Empty && recuperaPiatti) {
                StampaElementi(listView1, filename, 2, "", ref numm, encoding);
            } else {
                StampaElementi(listView1, filename, 3, textBox1.Text.ToUpper(), ref numm/*, checksum*/, encoding);
            }

            CrescDecr1 = !CrescDecr1;//così non mi sballa ordine quando lo riseleziono
            CrescDecr3 = !CrescDecr3;

            OrdinaElementi(nColonna, listView1, false, ref CrescDecr1, ref CrescDecr3); //li ordino per l'ultima categoria ordinata

            if (!ClienteProprietario && volte < 1) {//faccio backup solo se è cliente, non proprietario fa il backup solo la prima volta, perchè tanto poi non aggiungo più piatti.
                backup = new string[listView1.Items.Count, 4];//prima di cambiare faccio backup di come erano le cose
                BackupElementiSelezionatiEQta(backup, listView1);
            }

            if (volte > 0 && !ClienteProprietario)//volte deve essere maggiore sennò appena lo apro crasha
                RipristinaIlBackup(backup, listView1, darkmode);
            volte++;

        }
        private void button4_Click(object sender, EventArgs e)//elimina fisicamente
        {
            if (listView1.Items.Count > 0) {
                string heloo = "il piatto: ";
                string[] nomePiatto = new string[1];
                int y = listView1.SelectedItems.Count;
                bool selezione = false;

                if (y > 0) { //così capisco se ho selezionato qualcosa e mi faccio la stringa con tutti i nomi
                    selezione = true;
                    nomePiatto = new string[y];
                    for (int i = 0; i < y; i++) {
                        nomePiatto[i] = listView1.SelectedItems[i].Text;
                        heloo += $"{EliminaSpazi(nomePiatto[i])}, ";
                    }
                    heloo = heloo.Substring(0, heloo.Length - 2);
                } else
                    heloo = "tutti i piatti";

                DialogResult dialog = MessageBox.Show($"Così facendo perderai definitivamente {heloo}. Sicuro di volerlo fare?", "ELIMINAZIONE FISICA", MessageBoxButtons.YesNo);

                if (dialog == DialogResult.Yes) {
                    if (selezione) {
                        for (int i = 0; i < y; i++)
                            EliminaDefinitivamente(filename, ref numm, nomePiatto[i], encoding);
                    } else {
                        EliminaDefinitivamente(filename, ref numm, null/*,ref checksum*/, encoding);
                    }
                }
            } else
                MessageBox.Show("Non sono presenti piatti da eliminare");

            textBox1_TextChanged(sender, e);
        }
        private void button3_Click(object sender, EventArgs e)//recupera piatti
        {
            if (ClienteProprietario) {
                listView1.Items.Clear();
                if (!recuperaPiatti) { //è false di default
                    button2.Text = "RIPRISTINA";
                    button3.Text = "TORNA AI PIATTI ESISTENTI";
                } else {
                    button2.Text = "ELIMINA PIATTO ";
                    button3.Text = "RECUPERA/ ELIMINA FIS. PIATTI";
                }

                button1.Enabled = !button1.Enabled;//li inverto tutti
                recuperaPiatti = !recuperaPiatti;
                button4.Visible = !button4.Visible;

                textBox1_TextChanged(sender, e);
            }
        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            OrdinaElementi(e.Column, listView1, true, ref CrescDecr1, ref CrescDecr3);

            if (!ClienteProprietario)
                RipristinaIlBackup(backup, listView1, darkmode);
        }
        private void button5_Click(object sender, EventArgs e)//ok cliente
        {
            if (salvaOrdineSuFile) {
                using (StreamWriter uu = new StreamWriter(@"ordine.txt")) {
                    double prezzo = 0;
                    for (int i = 0; i < backup.GetLength(0); i++) {
                        if (backup[i, 3] == "Color.Yellow") { //se rientra nei selezionati
                            uu.WriteLine($"NOME: {backup[i, 0]} QTA: {backup[i, 1].PadRight(10)} PREZZO: € {backup[i, 2]}");
                            prezzo += (double.Parse(backup[i, 2]) * double.Parse(backup[i, 1]));
                        }
                    }
                    uu.WriteLine($"PREZZO TOTALE: € {prezzo}");
                }
            }

            using (StreamWriter uu = new StreamWriter(@"ordiniLista.csv", true))//così scrivo in append col "true" e se il file non c'è lo crea da solo
            {
                string daScrivere = "";
                for (int i = 0; i < backup.GetLength(0); i++) {
                    if (backup[i, 3] == "Color.Yellow")
                        daScrivere += ($"\"{backup[i, 0]}\";{backup[i, 1]};{backup[i, 2]};");
                }
                uu.WriteLine(daScrivere);
            }

            button5.Text = "OK";
            giaPremutoCreaListaCliente = false;
            listaSCONTRINO.Items.Clear();
            volte = 0;//così mi rifà lui da solo il backup della lista senza le mie modifche
            textBox1.Text = String.Empty;//sennò mi rifà il backup solo sulla ricerca nel caso in cui ci sia inserito qualcosa
            listView1.Enabled = true;

            Form3_Load(sender, e);
        }
        private void button6_Click(object sender, EventArgs e)//dark mode
        {
            Impostasiu.Show();
        }
        private void button7_Click(object sender, EventArgs e)//logout
        {
            ScriviFileChecksum(filenameCheck, filename);
            bohLogout = false;
            ModificaAggiungi.Close();
            this.Close();
        }
        public static void ScriviFileChecksum(string filenameCheck, string filename) {
            using (StreamWriter uu = new StreamWriter(filenameCheck)) {
                uu.WriteLine($"{GetMD5Checksum(filename)}");
            }
        }
        public static string GetMD5Checksum(string filename)//l'ho preso da stack overflow
        {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }
        public static int TrovaIndiceDentroListView(string nome, ListView listino) {
            for (int i = 0; i < listino.Items.Count; i++) {
                if (listino.Items[i].SubItems[0].Text == nome)
                    return i;
            }
            return -1;
        }
        public static void AggiornaBackup(string[,] backup, string nome, string qta) {
            for (int i = 0; i < backup.GetLength(0); i++) {
                if (backup[i, 0] == nome) {
                    if (qta == "0") {
                        backup[i, 1] = "";//così a video non stampo nulla
                        backup[i, 3] = "Color.White"; //così lascia colore di default
                    } else {
                        backup[i, 1] = qta;
                        backup[i, 3] = "Color.Yellow";//così poi lo seleziona in giallo
                    }
                    i = backup.GetLength(0);//così esco
                }
            }
        }
        public static void RipristinaIlBackup(string[,] backup, ListView listino, bool darkmode) {
            int ind;
            for (int i = 0; i < listino.Items.Count; i++) {
                ind = TrovaIndiceBackup(backup, listino.Items[i].SubItems[0].Text);
                listino.Items[i].SubItems[4].Text = backup[ind, 1];
                if (backup[ind, 3] == "Color.Yellow") { //se è selezionato
                    listino.Items[i].BackColor = Color.Yellow;
                    listino.Items[i].ForeColor = Color.Black;//perchè se c'è dark mode sennò sarebbe bianco
                } else {
                    if (darkmode) { //se c'è la dark mode attiva
                        listino.Items[i].BackColor = Color.FromArgb(37, 42, 64);
                        listino.Items[i].ForeColor = Color.White;
                    } else {
                        listino.Items[i].BackColor = Color.White;//qui non ripristino prezzo perchè mi serve solo per seconda list
                        listino.Items[i].ForeColor = Color.Black;
                    }

                }

            }
        }
        public static int TrovaIndiceBackup(string[,] backup, string nome) {
            for (int i = 0; i < backup.GetLength(0); i++) {
                if (backup[i, 0] == nome)
                    return i;
            }
            return -1;
        }
        public static void BackupElementiSelezionatiEQta(string[,] backup, ListView listino)//il backup lo faccio solo una volta, dopo se proprio lo aggiorno
        {
            for (int i = 0; i < listino.Items.Count; i++) {
                backup[i, 0] = listino.Items[i].SubItems[0].Text;

                if (listino.Items[i].SubItems[4].Text != null && listino.Items[i].SubItems[4].Text != "")
                    backup[i, 1] = listino.Items[i].SubItems[4].Text;
                else
                    backup[i, 1] = "";

                backup[i, 2] = listino.Items[i].SubItems[1].Text;//prezzo

                if (listino.Items[i].ForeColor != Color.Yellow)
                    backup[i, 3] = "Color.White";
                else
                    backup[i, 3] = "Color.Yellow";
            }
        }
        public static void OrdinaElementi(int colonna, ListView listuccina, bool OrdineAlfQuandRiseleziono, ref bool CrescDecr1, ref bool CrescDecr3) {
            if (colonna == 0) {
                if ((listuccina.Sorting == SortOrder.None || listuccina.Sorting == SortOrder.Descending) && OrdineAlfQuandRiseleziono)
                    listuccina.Sorting = SortOrder.Ascending;
                else if (OrdineAlfQuandRiseleziono) //così se riseleziono al form e ho ordinato alfabeticamente non mi sballa ordine
                    listuccina.Sorting = SortOrder.Descending;
            } else if (colonna == 1) {
                for (int i = 0; i < listuccina.Items.Count; i++) {
                    for (int j = i; j < listuccina.Items.Count; j++) {
                        if ((CrescDecr1) && (double.Parse(listuccina.Items[i].SubItems[1].Text) > double.Parse(listuccina.Items[j].SubItems[1].Text)))
                            ScambiaElementi(i, j, listuccina);
                        else if ((!CrescDecr1) && (int.Parse(listuccina.Items[i].SubItems[1].Text) < int.Parse(listuccina.Items[j].SubItems[1].Text)))
                            ScambiaElementi(i, j, listuccina);
                    }
                }
                CrescDecr1 = !CrescDecr1;
            } else if (colonna == 3) {
                for (int i = 0; i < listuccina.Items.Count; i++) {
                    for (int j = i; j < listuccina.Items.Count; j++) {
                        if ((CrescDecr3) && (RitornaIntPosizione(listuccina.Items[i].SubItems[3].Text) > RitornaIntPosizione(listuccina.Items[j].SubItems[3].Text)))
                            ScambiaElementi(i, j, listuccina);
                        else if ((!CrescDecr3) && (RitornaIntPosizione(listuccina.Items[i].SubItems[3].Text) < RitornaIntPosizione(listuccina.Items[j].SubItems[3].Text)))
                            ScambiaElementi(i, j, listuccina);
                    }
                }
                CrescDecr3 = !CrescDecr3;
            }
        }
        public static int RitornaIntPosizione(string pos) {
            if (pos == "ANTIPASTO") {
                return 0;
            } else if (pos == "PRIMO") {
                return 1;
            } else if (pos == "SECONDO") {
                return 2;
            } else if (pos == "DOLCE") {
                return 3;
            }

            return -1;
        }
        public static void ScambiaElementi(int ind1, int ind2, ListView listuccia) {
            string[] backup = new string[] { " ", " ", " ", " " };//da quel che ho visto non posso scambiare gli item quindi così
            string[] backup1 = new string[] { " ", " ", " ", " " };

            for (int i = 0; i < listuccia.Items[ind1].SubItems.Count - 1; i++) //parte backup
                backup[i] = listuccia.Items[ind1].SubItems[i].Text;

            for (int i = 0; i < listuccia.Items[ind2].SubItems.Count - 1; i++)
                backup1[i] = listuccia.Items[ind2].SubItems[i].Text;


            for (int i = 0; i < listuccia.Items[ind2].SubItems.Count - 1; i++) //parte in cui scambio
                listuccia.Items[ind2].SubItems[i].Text = backup[i];

            for (int i = 0; i < listuccia.Items[ind1].SubItems.Count - 1; i++)
                listuccia.Items[ind1].SubItems[i].Text = backup1[i];
        }
        public static void EliminaDefinitivamente(string filename, ref int numm, string SoloUnoDaEliminare/*,ref string checksum*/, Encoding encoding) {
            int nVolte = 0, posPunt = 0, IndiceUnicoDaEliminare = 0;
            string rigaTemp = "";
            bool dentroTesto = false;

            int[] indiciEliminati = new int[numm];

            if (SoloUnoDaEliminare != null) {//se ho selezionato un elemento
                trovaEliminati(indiciEliminati, filename, numm, true, ref IndiceUnicoDaEliminare, SoloUnoDaEliminare, encoding);
                nVolte = 1; //così mi fa ciclo una sola volta
            } else {
                trovaEliminati(indiciEliminati, filename, numm, false, ref IndiceUnicoDaEliminare, null, encoding);
            }

            for (int i = 0; i < indiciEliminati.Length; i++) //trovo quante volte dovrò fare il ciclo per eliminare
            {
                if (indiciEliminati[i] != -1 && SoloUnoDaEliminare == null) //seconda condizione messa per fare il ciclo solo una volta in caso di selezione di un solo elemento
                    nVolte++;
            }

            for (int i = 0; i < nVolte; i++) {
                if (SoloUnoDaEliminare != null) {
                    posPunt = IndiceUnicoDaEliminare;
                } else {
                    trovaEliminati(indiciEliminati, filename, numm, false, ref IndiceUnicoDaEliminare, null, encoding);
                    posPunt = indiciEliminati[0];//prendo sempre il primo anche perchè gli indici cambiano
                }

                do {
                    var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    dentroTesto = false;

                    if (posPunt + DimensioniRecord.Record < DimensioniRecord.Record * numm) //controllo di stare dentro testo
                    {
                        dentroTesto = true;
                        p.Seek(posPunt + DimensioniRecord.Record, SeekOrigin.Begin);//mi posiziono su riga sotto
                        using (BinaryReader reader = new BinaryReader(p, encoding)) {
                            rigaTemp = reader.ReadString();
                        }
                        p.Close();
                        var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        y.Seek(posPunt, SeekOrigin.Begin); //mi posiziono su robo da eliminare e lo sovrascrivo
                        using (BinaryWriter writer = new BinaryWriter(y, encoding)) {
                            writer.Write(rigaTemp);
                        }
                        y.Close();
                        posPunt += DimensioniRecord.Record;
                    } else {
                        p.Close();
                    }

                } while (dentroTesto);//c'è while perchè porto su tutti quelli che stanno sotto

                numm--;
            }


            var fileNuovo = new FileStream(@"menu.piattoTemp", FileMode.OpenOrCreate, FileAccess.ReadWrite); //copio solo fino a cose che esistono ancora su un file temporaneo
            var fileOriginale = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            BinaryReader vecchio = new BinaryReader(fileOriginale, encoding);
            BinaryWriter nuovo = new BinaryWriter(fileNuovo, encoding);

            fileOriginale.Seek(0, SeekOrigin.Begin);

            while (fileOriginale.Position < DimensioniRecord.Record * numm) {
                string temp;
                temp = vecchio.ReadString();
                nuovo.Write(temp);
            }

            fileNuovo.Close();
            fileOriginale.Close();


            SovrascrivereFile(@"menu.piattoTemp", filename);
        }
        public static void SovrascrivereFile(string fileTemp, string fileOrig) {
            FileInfo fi = new FileInfo(fileTemp);
            FileInfo newFi = new FileInfo(fileOrig);
            newFi.Delete();
            newFi = fi.CopyTo(fileOrig);
            fi.Delete();
        }
        public static void trovaEliminati(int[] indici, string filename, int cosiUsati, bool soloUno, ref int indiceSoloUno, string piattoSoloUno, Encoding encoding) {
            string[] fields;
            int indUsati = 0;
            for (int i = 0; i < indici.Length; i++)
                indici[i] = -1;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            while (p.Position < DimensioniRecord.Record * cosiUsati) {
                fields = reader.ReadString().Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (!bool.Parse(fields[0])) {
                    if (soloUno && fields[1] == piattoSoloUno) {//se voglio eliminare fisicamente un solo piatto
                        indiceSoloUno = Convert.ToInt32(p.Position) - DimensioniRecord.Record;
                        p.Position = DimensioniRecord.Record * cosiUsati; //così esco da ciclo
                    } else if (!soloUno) {
                        indici[indUsati] = Convert.ToInt32(p.Position) - DimensioniRecord.Record;
                        indUsati++;
                    }
                }
            }
            p.Close();
        }
        public static void eliminaOripristinaPiatti(int inizioRecord, bool eliminaRipristina, string filename/*, ref string checksum*/, Encoding encoding) {
            //elimina o ripristina == true allora sto recuperando un piatto
            string[] fields;

            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            p.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryReader reader = new BinaryReader(p, encoding)) {
                fields = reader.ReadString().Split(';');
            }
            p.Close();

            var y = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            y.Seek(inizioRecord, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(y, encoding)) {
                string totale = $"{$"{eliminaRipristina}".PadRight(DimensioniRecord.PadEliminato)};{fields[1]};{fields[2]};{fields[3]};{fields[4]};".PadRight(DimensioniRecord.Record - 1);
                writer.Write(totale);
            }
            y.Close();
        }
        public static void StampaElementi(ListView listino, string filename, int ricerca, string testoRicerca, ref int numm/*,string checksum*/, Encoding encoding) {
            listino.Items.Clear();

            string[] fields;
            string[] fieldsRidotti;
            string[] ingredienti;

            numm = 0;

            try {
                var f = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                f.Seek(0, SeekOrigin.Begin);
                using (BinaryReader leggiNomi = new BinaryReader(f, encoding)) {
                    while (f.Position < f.Length) {
                        numm++;//sarebbe quello che stava nel vecchio file dei numeri

                        fields = leggiNomi.ReadString().Split(';');

                        fieldsRidotti = new string[fields.Length - 1]; //così non stampo a video il bool dell'eleminazione
                        for (int i = 0; i < fields.Length - 1; i++)
                            fieldsRidotti[i] = fields[i + 1];

                        ingredienti = fieldsRidotti[2].Split(','); //per togliere spazi in eccesso
                        fieldsRidotti[2] = "";
                        for (int i = 0; i < ingredienti.Length; i++) {
                            ingredienti[i] = $"{EliminaSpazi(ingredienti[i])}";
                            fieldsRidotti[2] += $"{ingredienti[i]}, ";
                        }
                        fieldsRidotti[2] = fieldsRidotti[2].Substring(0, fieldsRidotti[2].Length - 2);


                        if (int.Parse(fieldsRidotti[3]) == 0) {
                            fieldsRidotti[3] = "ANTIPASTO";
                        } else if (int.Parse(fieldsRidotti[3]) == 1) {
                            fieldsRidotti[3] = "PRIMO";
                        } else if (int.Parse(fieldsRidotti[3]) == 2) {
                            fieldsRidotti[3] = "SECONDO";
                        } else if (int.Parse(fieldsRidotti[3]) == 3) {
                            fieldsRidotti[3] = "DOLCE";
                        }

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
            } catch {
                MessageBox.Show("File 'piatti.ristorante' corrotto. Il programma si chiuderà");
                Environment.Exit(1);
            }

        }
        public static string EliminaSpazi(string elemento) {
            while (true) {
                if (elemento.Substring(elemento.Length - 1, 1) == " ") {
                    elemento = elemento.Substring(0, elemento.Length - 1);
                } else {
                    break;
                }
            }
            return elemento;
        }
        public static int cercaPiatto(string nomePiatto, string filename, Encoding encoding) {
            int pos = -1;
            string[] fields;
            var p = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(p, encoding);
            p.Seek(0, SeekOrigin.Begin);

            while (p.Position < p.Length) {
                fields = reader.ReadString().Split(';'); //0=boolEsistenza 1=nome 2=prezo 3=1ingredienti 4=posizione
                if (fields[1] == nomePiatto) {
                    pos = Convert.ToInt32(p.Position);
                    break;
                }
            }
            p.Close();

            return pos;
        }
    }
}
