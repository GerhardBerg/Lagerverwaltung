using Lagerverwaltung.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public partial class Warenausgang : Window
    {
        public Warenausgang()
        {
            InitializeComponent();
            LoadFilters();
            LoadWarenausgang();
        }
        private void LoadFilters()
        {
            // Projekt-Liste
            var dtProjekt = DatabaseHelper.ExecuteQuery("SELECT DISTINCT Projekt FROM Warenausgang");
            cmbProjektFilter.Items.Clear();
            cmbProjektFilter.Items.Add("Alle");
            foreach (DataRow r in dtProjekt.Rows)
                cmbProjektFilter.Items.Add(r["Projekt"].ToString());
            cmbProjektFilter.SelectedIndex = 0;

            // Kunde-Liste
            var dtKunde = DatabaseHelper.ExecuteQuery("SELECT DISTINCT Kunde FROM Warenausgang");
            cmbKundeFilter.Items.Clear();
            cmbKundeFilter.Items.Add("Alle");
            foreach (DataRow r in dtKunde.Rows)
                cmbKundeFilter.Items.Add(r["Kunde"].ToString());
            cmbKundeFilter.SelectedIndex = 0;
        }
        private void LoadWarenausgang()
        {
            string query = "SELECT lfd_Nr, Buchungsdatum, Teilenummer, Menge, Typ, Wert, Bezeichnung, Projekt, Kunde, Einzelpreis, Status FROM Warenausgang WHERE 1=1";
            var parameters = new List<SqlParameter>();

            if (cmbProjektFilter.SelectedItem != null && cmbProjektFilter.SelectedItem.ToString() != "Alle")
            {
                query += " AND Projekt = @Projekt";
                parameters.Add(new SqlParameter("@Projekt", cmbProjektFilter.SelectedItem.ToString()));
            }
            if (cmbKundeFilter.SelectedItem != null && cmbKundeFilter.SelectedItem.ToString() != "Alle")
            {
                query += " AND Kunde = @Kunde";
                parameters.Add(new SqlParameter("@Kunde", cmbKundeFilter.SelectedItem.ToString()));
            }

            var dt = DatabaseHelper.ExecuteQuery(query, parameters.ToArray());

            // Neue Spalte "Gesamtpreis" berechnen
            dt.Columns.Add("Gesamtpreis", typeof(decimal));
            foreach (DataRow row in dt.Rows)
            {
                decimal menge = row["Menge"] != DBNull.Value ? Convert.ToDecimal(row["Menge"]) : 0;
                decimal preis = row["Einzelpreis"] != DBNull.Value ? Convert.ToDecimal(row["Einzelpreis"]) : 0;
                row["Gesamtpreis"] = menge * preis;
            }

            dgWarenausgang.ItemsSource = dt.DefaultView;

            // Summe berechnen
            decimal summe = dt.AsEnumerable().Sum(r => r.Field<decimal>("Gesamtpreis"));
            txtSumme.Text = $"Summe: {summe:N2} €";
        }
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadWarenausgang();
        }
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbProjektFilter.SelectedIndex = 0;
            cmbKundeFilter.SelectedIndex = 0;
            LoadWarenausgang();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Fenster schließen
        }
    }
}
