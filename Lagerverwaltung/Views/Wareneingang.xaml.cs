using Lagerverwaltung.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lagerverwaltung.Views
{
    /// <summary>
    /// Interaktionslogik für Wareneingang.xaml
    /// </summary>
    public partial class Wareneingang : Window
    {
        private List<BestellungViewModel> bestellungen;
        private List<BestellungViewModel> wareneingang;
        public Wareneingang()
        {
            InitializeComponent();
            LadeBestellungen();
            LadeWareneingang();
        }

        private void LadeBestellungen()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM bestellung WHERE Status = 'bestellt'");
            bestellungen = (from System.Data.DataRow row in dt.Rows
                            select new BestellungViewModel
                            {
                                lfd_Nr = Convert.ToInt32(row["lfd_Nr"]),
                                Bestelldatum = Convert.ToDateTime(row["Bestelldatum"]),
                                Bestellmenge = Convert.ToInt32(row["Bestellmenge"]),
                                Typ = row["Typ"].ToString(),
                                Wert = row["Wert"].ToString(),
                                Bezeichnung = row["Bezeichnung"].ToString(),
                                Preis = Convert.ToDecimal(row["Preis"]),
                                Lieferant = row["Lieferant"].ToString(),
                                Bestellnummer = row["Bestellnummer"].ToString(),
                                Status = row["Status"].ToString(),
                                IsSelected = false
                            }).ToList();

            dgBestellungen.ItemsSource = bestellungen;
        }
        private void LadeWareneingang()
        {
            DataTable dt2 = DatabaseHelper.ExecuteQuery("SELECT * FROM Wareneingang WHERE Status = 'nicht gebucht'");
            wareneingang = (from System.Data.DataRow row in dt2.Rows
                            select new BestellungViewModel
                            {
                                lfd_Nr = Convert.ToInt32(row["lfd_Nr"]),
                                Bestelldatum = Convert.ToDateTime(row["Bestelldatum"]),
                                Bestellmenge = Convert.ToInt32(row["Bestellmenge"]),
                                Typ = row["Typ"].ToString(),
                                Wert = row["Wert"].ToString(),
                                Bezeichnung = row["Bezeichnung"].ToString(),
                                Preis = Convert.ToDecimal(row["Preis"]),
                                Lieferant = row["Lieferant"].ToString(),
                                Bestellnummer = row["Bestellnummer"].ToString(),
                                Status = row["Status"].ToString(),
                                Lieferdatum = row["Lieferdatum"] as DateTime?,
                                IsSelected = false
                            }).ToList();

            dgWareneingaengeOffen.ItemsSource = wareneingang;
        }
        private void LagerUebernehmen_Click(object sender, RoutedEventArgs e)
        {
            var ausgewählte = wareneingang.Where(x => x.IsSelected).ToList();

            if (ausgewählte.Count == 0)
            {
                MessageBox.Show("Bitte mindestens einen Wareneingang auswählen.");
                return;
            }
            int wertCount = 0;
            foreach (var wareneingang in ausgewählte)
            {
                //als erstes schauen ob es den Typ schon gibt, wenn nicht Eingabefenster von Teiletypnummer (E,010=Widerstand,001=erster Typ wird automatisch vergeben)
                // 1. Prüfen, ob Artikel (Typ+Wert) schon im Lager existiert
                string checkSql = "SELECT COUNT(*) FROM Lager WHERE Typ = @Typ";
                int vorhandene = DatabaseHelper.ExecuteScalar<int>(
                    checkSql,
                    new SqlParameter("@Typ", wareneingang.Typ)
                );
                string teilenummer = null;
                if (vorhandene == 0)
                {
                    // 2. Benutzer nach Teilenummer fragen
                    var dialog = new TeileNummerDialog(); // eigenes kleines WPF-Fenster
                    if (dialog.ShowDialog() == true)
                    {
                        teilenummer = dialog.Teilenummer + "001";
                    }
                    decimal einzelpreis = wareneingang.Preis / wareneingang.Bestellmenge;

                    string sqlInsert = @"INSERT INTO Lager (Teilenummer,Typ, Wert, Menge, Bezeichnung, Einzelpreis, letzte_Aktualisierung) 
                                     VALUES (@Teilenummer, @Typ, @Wert,  @Menge, @Bezeichnung, @Preis, @Datum)";
                    DatabaseHelper.ExecuteNonQuery(sqlInsert,
                        new SqlParameter("@Teilenummer", teilenummer),
                        new SqlParameter("@Typ", wareneingang.Typ),
                        new SqlParameter("@Wert", wareneingang.Wert),
                        new SqlParameter("@Bezeichnung", wareneingang.Bezeichnung),
                        new SqlParameter("@Menge", wareneingang.Bestellmenge),
                        new SqlParameter("@Preis", einzelpreis),
                        new SqlParameter("@Datum", DateTime.Now));
                }
                else
                {
                    //Typ ist vorhanden
                    string checkSql2 = "SELECT COUNT(*) FROM Lager WHERE Typ = @Typ AND Wert = @Wert";
                    wertCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar<int>(checkSql2,
                        new SqlParameter("@Typ", wareneingang.Typ),
                        new SqlParameter("@Wert", wareneingang.Wert)));

                    if (wertCount > 0)
                    {
                        // 3a. Wenn Typ + Wert vorhanden → Menge addieren
                        string sqlUpdate = @"UPDATE Lager 
                                        SET Menge = Menge + @Menge 
                                        WHERE Typ = @Typ AND Wert = @Wert";
                        DatabaseHelper.ExecuteNonQuery(sqlUpdate,
                            new SqlParameter("@Menge", wareneingang.Bestellmenge),
                            new SqlParameter("@Typ", wareneingang.Typ),
                            new SqlParameter("@Wert", wareneingang.Wert));
                    }
                    {
                        // 3b. Wenn Typ da, aber Wert nicht → neuen Artikel anlegen
                        // 1. Präfix der Teilenummer holen (z.B. "E021")
                        //   Angenommen: Du hast mindestens eine vorhandene Teilenummer in Lager für diesen Typ
                        string prefix = DatabaseHelper.ExecuteScalar<string>(
                            "SELECT TOP 1 LEFT(Teilenummer, LEN(Teilenummer) - 3) " +
                            "FROM Lager WHERE Typ = @Typ ORDER BY Teilenummer DESC",
                            new SqlParameter("@Typ", wareneingang.Typ)
                        );

                        // 2. Höchste Zahl am Ende holen
                        int maxNummer = DatabaseHelper.ExecuteScalar<int>(
                            "SELECT ISNULL(MAX(CAST(RIGHT(Teilenummer, 3) AS INT)), 0) " +
                            "FROM Lager WHERE Typ = @Typ",
                            new SqlParameter("@Typ", wareneingang.Typ)
                        );

                        // 3. Neue Nummer berechnen
                        int neueNummer = maxNummer + 1;

                        // 4. Neue Teilenummer zusammensetzen → Präfix + neueNummer
                        string neueTeilenummer = prefix + neueNummer.ToString("D3");

                        string sqlInsert = @"INSERT INTO Lager (Teilenummer,Typ, Wert, Menge, Bezeichnung, Einzelpreis, letzte_Aktualisierung) 
                                     VALUES (@Teilenummer, @Typ, @Wert,  @Menge, @Bezeichnung, @Preis, @Datum)";
                        DatabaseHelper.ExecuteNonQuery(sqlInsert,
                            new SqlParameter("@Teilenummer", neueTeilenummer),
                            new SqlParameter("@Typ", wareneingang.Typ),
                            new SqlParameter("@Wert", wareneingang.Wert),
                            new SqlParameter("@Bezeichnung", wareneingang.Bezeichnung),
                            new SqlParameter("@Menge", wareneingang.Bestellmenge),
                            new SqlParameter("@Preis", wareneingang.Preis),
                            new SqlParameter("@Datum", DateTime.Now));
                    }

                }


                // Zum Schluss: Status im Wareneingang auf "gebucht" setzen
                string sqlUpdateStatus = "UPDATE Wareneingang SET Status = 'gebucht' WHERE lfd_Nr = @Id";
                DatabaseHelper.ExecuteNonQuery(sqlUpdateStatus,
                    new SqlParameter("@Id", wareneingang.lfd_Nr));



            }
            LadeWareneingang();
        }
        private void BtnUebernehmen_Click(object sender, RoutedEventArgs e)
        {
            var ausgewaehlte = bestellungen.Where(b => b.IsSelected).ToList();

            if (ausgewaehlte.Count == 0)
            {
                MessageBox.Show("Bitte mindestens eine Bestellung auswählen.");
                return;
            }

            foreach (var bestellung in ausgewaehlte)
            {
                if (bestellung.Lieferdatum == null)
                {
                    MessageBox.Show($"Bitte Lieferdatum für Bestellung {bestellung.lfd_Nr} eingeben.");
                    return;
                }

                string insertSql = $@"
    INSERT INTO Wareneingang 
    ( Bestelldatum, Bestellmenge, Typ, Wert, Bezeichnung, Preis, Lieferant, Bestellnummer, Status, Lieferdatum)
    VALUES 
    ('{bestellung.Bestelldatum:yyyy-MM-dd}', {bestellung.Bestellmenge},
     '{bestellung.Typ}', '{bestellung.Wert}', '{bestellung.Bezeichnung}', {bestellung.Preis.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
     '{bestellung.Lieferant}', '{bestellung.Bestellnummer}', 'nicht gebucht', '{bestellung.Lieferdatum:yyyy-MM-dd}')";

                DatabaseHelper.ExecuteNonQuery(insertSql);

                string updateStatusSql = $@"
        UPDATE bestellung 
        SET Status = 'geliefert' 
        WHERE lfd_Nr = {bestellung.lfd_Nr}";

                DatabaseHelper.ExecuteNonQuery(updateStatusSql);
            }

            MessageBox.Show("Wareneingang erfolgreich übernommen.");
            LadeBestellungen();
            LadeWareneingang();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Checkbox für "Alle auswählen"
        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (bestellungen == null) return;
            foreach (var b in bestellungen)
                b.IsSelected = true;
            dgBestellungen.Items.Refresh();
        }
        private void chkSelectAll_Checked2(object sender, RoutedEventArgs e)
        {
            if (wareneingang == null) return;
            foreach (var b in wareneingang)
                b.IsSelected = true;
            dgWareneingaengeOffen.Items.Refresh();
        }
        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (bestellungen == null) return;
            foreach (var b in bestellungen)
                b.IsSelected = false;
            dgBestellungen.Items.Refresh();
        }
        private void chkSelectAll_Unchecked2(object sender, RoutedEventArgs e)
        {
            if (wareneingang == null) return;
            foreach (var b in wareneingang)
                b.IsSelected = false;
            dgWareneingaengeOffen.Items.Refresh();
        }
    }
    public class BestellungViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private DateTime? _lieferdatum;
        public DateTime? Lieferdatum
        {
            get => _lieferdatum;
            set
            {
                if (_lieferdatum != value)
                {
                    _lieferdatum = value;
                    OnPropertyChanged(nameof(Lieferdatum));
                }
            }
        }

        public int lfd_Nr { get; set; }
        public DateTime Bestelldatum { get; set; }
        public int Bestellmenge { get; set; }
        public string Typ { get; set; }
        public string Wert { get; set; }
        public string Bezeichnung { get; set; }
        public decimal Preis { get; set; }
        public string Lieferant { get; set; }
        public string Bestellnummer { get; set; }
        public string Status { get; set; }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
