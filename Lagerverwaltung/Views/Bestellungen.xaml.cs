using Lagerverwaltung.Data;
using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für Bestellungen.xaml
    /// </summary>
    public partial class Bestellungen : Window
    {
        public Bestellungen()
        {
            InitializeComponent();
            LadeTypen();
            LadeBestellungen();
        }
        private void LadeTypen()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT DISTINCT Typ FROM Lager WHERE Typ IS NOT NULL");
            cbTyp.Items.Clear();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                cbTyp.Items.Add(row["Typ"].ToString());
            }
        }
        private void LadeBestellungen()
        {
            dgBestellungen.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Bestellung").DefaultView;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Speichern_Click(object sender, RoutedEventArgs e)
        {
            string typ = "";
            //if (cbTyp.SelectedItem == null)
            //{
            //    string typNeu = cbTyp.Text;
            //}
            //else
            //{
            //    typ = cbTyp.SelectedItem?.ToString() ?? "";
            //}
            try
                {
                    string preisText = txtPreis.Text.Replace(',', '.');

                    string sql = $@"
                    INSERT INTO Bestellung 
                    (Bestelldatum, Bestellmenge, Typ, Wert, Bezeichnung, Preis, Lieferant, Bestellnummer,status)
                    VALUES
                    ('{dpBestelldatum.SelectedDate:yyyy-MM-dd}',
                     {int.Parse(txtBestellmenge.Text)},
                     '{typ = cbTyp.SelectedItem?.ToString() ?? cbTyp.Text}',
                     '{txtWert.Text}',
                     '{txtBezeichnung.Text}',
                     '{preisText}',
                     
                     '{txtLieferant.Text}',
                     '{txtBestellnummer.Text}',
                     'bestellt')";

                    DatabaseHelper.ExecuteNonQuery(sql);
                    LadeBestellungen();
                    MessageBox.Show("Bestellung gespeichert.");
                    EingabeLeeren();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler beim Speichern: " + ex.Message);
                    //'{decimal.Parse(txtPreis.Text)}',
                }
        }
        private void EingabeLeeren()
        {
            txtBestellmenge.Text = "";
            txtWert.Text = "";
            txtBezeichnung.Text = "";
            txtPreis.Text = "";
            txtLieferant.Text = "";
            txtBestellnummer.Text = "";
            dpBestelldatum.SelectedDate = null;
            cbTyp.SelectedIndex = -1;
            dgBestellungen.SelectedItem = null;
            cbTyp.SelectedItem = null;
            cbTyp.Text = "";
            dpBestelldatum.Focus();

        }
    }
}
