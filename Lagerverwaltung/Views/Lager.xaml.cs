using Lagerverwaltung.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lagerverwaltung.Views
{
    public partial class Lager : Window
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            LoadLager();
        }
        private void LoadFilters()
        {
            // Typ laden
            var dtTyp = DatabaseHelper.ExecuteQuery("SELECT DISTINCT Typ FROM Lager");
            var typList = dtTyp.AsEnumerable()
                               .Select(r => r["Typ"].ToString())
                               .ToList();

            typList.Insert(0, "Bitte wählen");
            cmbTypFilter.ItemsSource = typList;
            cmbTypFilter.SelectedIndex = 0;

            // Wert und Bezeichnung erstmal leer setzen
            cmbWertFilter.ItemsSource = null;
            cmbBezeichnungFilter.ItemsSource = null;
        }
        private void LoadDependentFilter(string selectedTyp)
        {
            if (selectedTyp == "Bitte wählen")
            {
                cmbWertFilter.ItemsSource = null;
                cmbBezeichnungFilter.ItemsSource = null;
                return;
            }

            // Werte laden
            var dtWert = DatabaseHelper.ExecuteQuery(
                "SELECT DISTINCT Wert FROM Lager WHERE Typ = @Typ",
                new SqlParameter("@Typ", selectedTyp));

            var wertList = dtWert.AsEnumerable()
                                 .Select(r => r["Wert"].ToString())
                                 .ToList();

            wertList.Insert(0, "Bitte wählen");

            cmbWertFilter.ItemsSource = wertList;
            cmbWertFilter.SelectedIndex = 0;

            // Bezeichnungen auch zurücksetzen
            cmbBezeichnungFilter.ItemsSource = null;
        }
        private void LoadDependentFilter2(string selectedTyp)
        {
            if (selectedTyp == "Bitte wählen")
            {
                cmbBezeichnungFilter.ItemsSource = null;
                return;
            }

            // Werte laden
            var dtWert = DatabaseHelper.ExecuteQuery(
                "SELECT DISTINCT Bezeichnung FROM Lager WHERE Typ = @Typ and Wert = @Wert",
                new SqlParameter("@Wert", selectedTyp),
                new SqlParameter("@Typ", cmbTypFilter.SelectedItem));

            var wertList = dtWert.AsEnumerable()
                                 .Select(r => r["Bezeichnung"].ToString())
                                 .ToList();

            cmbBezeichnungFilter.ItemsSource = wertList;

        }
        private void LoadLager()
        {
            string query = "SELECT * FROM Lager WHERE 1=1";
            var parameters = new List<SqlParameter>();

            if (cmbTypFilter.SelectedItem != null && cmbTypFilter.SelectedItem.ToString() != "Alle")
            {
                query += " AND Typ = @Typ";
                parameters.Add(new SqlParameter("@Typ", cmbTypFilter.SelectedItem.ToString()));
            }
            if (cmbWertFilter.SelectedItem != null && cmbWertFilter.SelectedItem.ToString() != "Alle")
            {
                query += " AND Wert = @Wert";
                parameters.Add(new SqlParameter("@Wert", cmbWertFilter.SelectedItem.ToString()));
            }
            if (cmbBezeichnungFilter.SelectedItem != null && cmbBezeichnungFilter.SelectedItem.ToString() != "Alle")
            {
                query += " AND Bezeichnung = @Bezeichnung";
                parameters.Add(new SqlParameter("@Bezeichnung", cmbBezeichnungFilter.SelectedItem.ToString()));
            }
            // Daten abholen
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());

            // In Liste von LagerItem umwandeln
            var lagerListe = dt.AsEnumerable().Select(r => new LagerItem
            {
                IsSelected = false,
                Teilenummer = r["Teilenummer"].ToString(),
                Typ = r["Typ"].ToString(),
                Wert = r["Wert"].ToString(),
                Bezeichnung = r["Bezeichnung"].ToString(),
                Menge = Convert.ToInt32(r["Menge"]),
                Einzelpreis = Convert.ToDecimal(r["Einzelpreis"]),
                Lagermenge_min = r["Lagermenge_min"] != DBNull.Value ? Convert.ToInt32(r["Lagermenge_min"]) : 0,
                Lagermenge_max = r["Lagermenge_max"] != DBNull.Value ? Convert.ToInt32(r["Lagermenge_max"]) : 0,
                Lagerplatz = r["Lagerplatz"].ToString(),
                Letzte_Aktualisierung = r["letzte_Aktualisierung"] != DBNull.Value
                                              ? Convert.ToDateTime(r["letzte_Aktualisierung"])
                                              : DateTime.MinValue

            }).ToList();
            // Liste als ItemsSource setzen
            dgLager.ItemsSource = lagerListe;

            //dgLager.ItemsSource = DatabaseHelper.ExecuteQuery(query, parameters.ToArray()).DefaultView;
        }
        bool aktualisiert = false;
        bool zurücksetzen = false;
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxName = "";
            if (sender is ComboBox comboBox)
            {
                comboBoxName = comboBox.Name;
            }
            if (comboBoxName == "cmbTypFilter" && !aktualisiert && !zurücksetzen)
            {
                aktualisiert = true;
                string selectedTyp = cmbTypFilter.SelectedItem.ToString();
                LoadDependentFilter(selectedTyp);
            }
            else if (comboBoxName == "cmbWertFilter" && !aktualisiert && !zurücksetzen)
            {
                aktualisiert = true;
                string selectedTyp = cmbWertFilter.SelectedItem.ToString();
                LoadDependentFilter2(selectedTyp);
            }
            else if (comboBoxName == "cmbBezeichnungFilter" && !aktualisiert && !zurücksetzen)
            {

            }
            LoadLager();
            aktualisiert = false;
        }

        private List<LagerItem> lagerListe = new List<LagerItem>();
        public Lager()
        {
            InitializeComponent();
            LoadLagerGrid();
            LoadFilterLists();
        }
        private void LoadLagerGrid()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Lager");
            lagerListe = dt.AsEnumerable().Select(r => new LagerItem
            {
                IsSelected = false,
                Teilenummer = r["Teilenummer"].ToString(),
                Typ = r["Typ"].ToString(),
                Wert = r["Wert"].ToString(),
                Menge = Convert.ToInt32(r["Menge"]),
                Bezeichnung = r["Bezeichnung"].ToString(),
                Einzelpreis = Convert.ToDecimal(r["Einzelpreis"]),
                Lagermenge_min = r["Lagermenge_min"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(r["Lagermenge_min"]),
                Lagermenge_max = r["Lagermenge_max"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(r["Lagermenge_max"]),
                //Maximum = Convert.ToInt32(r["Lagermenge_max"]),
                Lagerplatz = r["Lagerplatz"].ToString(),
                Letzte_Aktualisierung = Convert.ToDateTime(r["letzte_Aktualisierung"])
            }).ToList();

            dgLager.ItemsSource = lagerListe;
        }
        private void LoadFilterLists()
        {
            var typen = lagerListe.Select(x => x.Typ).Distinct().OrderBy(x => x).ToList();
            cmbTypFilter.ItemsSource = typen;

            var werte = lagerListe.Select(x => x.Wert).Distinct().OrderBy(x => x).ToList();
            cmbWertFilter.ItemsSource = werte;

            var bezeichnungen = lagerListe.Select(x => x.Bezeichnung).Distinct().OrderBy(x => x).ToList();
            cmbBezeichnungFilter.ItemsSource = bezeichnungen;
        }
        private void BtnAusbuchen_Click(object sender, RoutedEventArgs e)
        {
            if (dgLager.SelectedItem is LagerItem selectedItem)
            {
                WarenausgangDialog dialog = new WarenausgangDialog(
                    selectedItem.Teilenummer, selectedItem.Typ, selectedItem.Wert, selectedItem.Bezeichnung, selectedItem.Menge);
                if (dialog.ShowDialog() == true)
                {
                    int ausbuchMenge = dialog.Menge;
                    if (ausbuchMenge > selectedItem.Menge) ausbuchMenge = selectedItem.Menge;

                    string projekt = dialog.Projekt;
                    string kunde = dialog.Kunde;

                    // Lager aktualisieren
                    int neueMenge = selectedItem.Menge - ausbuchMenge;
                    string sqlLager = @"UPDATE Lager 
                                    SET Menge = " + neueMenge + @", letzte_Aktualisierung = '" + DateTime.Now.ToString("yyyy-MM-dd") + @"'
                                    WHERE Teilenummer = '" + selectedItem.Teilenummer.Replace("'", "''") + "'";
                    DatabaseHelper.ExecuteQuery(sqlLager);

                    // Warenausgang eintragen
                    string sqlAusgang = @"INSERT INTO Warenausgang (Buchungsdatum, Teilenummer, Menge, Typ, Wert, Bezeichnung, Projekt, Kunde, Einzelpreis, Status)
                                      VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd") + @"',
                                              '" + selectedItem.Teilenummer.Replace("'", "''") + @"',
                                              " + ausbuchMenge + @",
                                                '" + selectedItem.Typ.Replace("'", "''") + @"',
                                                '" + selectedItem.Wert.Replace("'", "''") + @"',
                                                '" + selectedItem.Bezeichnung.Replace("'", "''") + @"',
                                              '" + projekt.Replace("'", "''") + @"',
                                              '" + kunde.Replace("'", "''") + @"',
                                                " + selectedItem.Einzelpreis.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + @",
                                              'gebucht')";
                    DatabaseHelper.ExecuteQuery(sqlAusgang);
                }
                MessageBox.Show("Ausgewählte Positionen wurden ausgebucht.");
                Filter_SelectionChanged(null, null);
            }
            //    var ausgewaehlt = ((List<LagerItem>)dgLager.ItemsSource)
            //        .Where(x => x.IsSelected)
            //        .ToList();

            //if (ausgewaehlt.Count == 0)
            //{
            //    MessageBox.Show("Bitte mindestens eine Position auswählen!");
            //    return;
            //}

            //foreach (var row in ausgewaehlt)
            //{
                //WarenausgangDialog dialog = new WarenausgangDialog(
                //    row.Teilenummer, row.Typ, row.Wert, row.Bezeichnung, row.Menge);
                //if (dialog.ShowDialog() != true) continue;
                //int ausbuchMenge = dialog.Menge;
                //if (ausbuchMenge > row.Menge) ausbuchMenge = row.Menge;

                //string projekt = dialog.Projekt;
                //string kunde = dialog.Kunde;

                // Lager aktualisieren
                //int neueMenge = row.Menge - ausbuchMenge;
                //string sqlLager = @"UPDATE Lager 
                //                    SET Menge = " + neueMenge + @", letzte_Aktualisierung = '" + DateTime.Now.ToString("yyyy-MM-dd") + @"'
                //                    WHERE Teilenummer = '" + row.Teilenummer.Replace("'", "''") + "'";
                //DatabaseHelper.ExecuteQuery(sqlLager);

                // Warenausgang eintragen
                //string sqlAusgang = @"INSERT INTO Warenausgang (Buchungsdatum, Teilenummer, Menge, Projekt, Kunde, Status)
                //                      VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd") + @"',
                //                              '" + row.Teilenummer.Replace("'", "''") + @"',
                //                              " + ausbuchMenge + @",
                //                              '" + projekt.Replace("'", "''") + @"',
                //                              '" + kunde.Replace("'", "''") + @"',
                //                              'gebucht')";
                //DatabaseHelper.ExecuteQuery(sqlAusgang);
            //}

            //MessageBox.Show("Ausgewählte Positionen wurden ausgebucht.");
            //Filter_SelectionChanged(null, null);
        }
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            zurücksetzen = true;
            // Auswahl der Filter zurücksetzen
            cmbTypFilter.SelectedIndex = -1;
            cmbWertFilter.SelectedIndex = -1;
            cmbBezeichnungFilter.SelectedIndex = -1;

            // Grid wieder vollständig laden
            LoadLager();
            zurücksetzen = false;
        }
        private void BtnPositionAendern_Click(object sender, RoutedEventArgs e)
        {
            if (dgLager.SelectedItem is LagerItem selectedItem)
            {
                var editWindow = new LagerEditWindow(selectedItem);
                if (editWindow.ShowDialog() == true)
                {
                    // Änderungen ins DataGrid übernehmen
                    dgLager.Items.Refresh();

                    // Optional: Änderungen direkt in der Datenbank speichern
                    string updateQuery = @"
                    UPDATE Lager SET
                        Teilenummer = @Teilenummer,
                        Typ = @Typ,
                        Wert = @Wert,
                        Menge = @Menge,
                        Bezeichnung = @Bezeichnung,
                        Einzelpreis = @Einzelpreis,
                        Lagermenge_min = @Lagermenge_min,
                        Lagermenge_max = @Lagermenge_max,
                        Lagerplatz = @Lagerplatz,
                        Letzte_Aktualisierung = @Letzte_Aktualisierung
                    WHERE Teilenummer = @Teilenummer";

                    DatabaseHelper.ExecuteNonQuery(updateQuery,
                        new SqlParameter("@Teilenummer", selectedItem.Teilenummer),
                        new SqlParameter("@Typ", selectedItem.Typ),
                        new SqlParameter("@Wert", selectedItem.Wert),
                        new SqlParameter("@Menge", selectedItem.Menge),
                        new SqlParameter("@Bezeichnung", selectedItem.Bezeichnung),
                        new SqlParameter("@Einzelpreis", selectedItem.Einzelpreis),
                        new SqlParameter("@Lagermenge_min", selectedItem.Lagermenge_min),
                        new SqlParameter("@Lagermenge_max", selectedItem.Lagermenge_max),
                        new SqlParameter("@Lagerplatz", selectedItem.Lagerplatz),
                        new SqlParameter("@Letzte_Aktualisierung", DateTime.Now));
                }
            }
        }       

        private void BtnSchliessen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    public class LagerItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public string Teilenummer { get; set; }
        public string Typ { get; set; }
        public string Wert { get; set; }
        public int Menge { get; set; }
        public string Bezeichnung { get; set; }
        public decimal Einzelpreis { get; set; }
        public int Lagermenge_min { get; set; }
        public int Lagermenge_max { get; set; }
        public string Lagerplatz { get; set; }
        public DateTime Letzte_Aktualisierung { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
