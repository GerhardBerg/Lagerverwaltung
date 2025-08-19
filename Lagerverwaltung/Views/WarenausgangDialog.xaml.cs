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
    /// Interaktionslogik für WarenausgangDialog.xaml
    /// </summary>
    public partial class WarenausgangDialog : Window
    {
        public int Menge { get; private set; }
        public string Projekt { get; private set; }
        public string Kunde { get; private set; }
        public WarenausgangDialog(string teilenummer, string typ, string wert, string bezeichnung, int maxMenge)
        {
            InitializeComponent();

            txtPositionInfo.Text = $"Teilenummer: {teilenummer}, Typ: {typ}, Wert: {wert}, Bezeichnung: {bezeichnung}";
            txtMenge.Text = maxMenge.ToString();
        }
        private void BtnBuchen_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtMenge.Text, out int menge) || menge <= 0)
            {
                MessageBox.Show("Bitte eine gültige Menge eingeben.");
                return;
            }

            Menge = menge;
            Projekt = txtProjekt.Text.Trim();
            Kunde = txtKunde.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }
    }
}
