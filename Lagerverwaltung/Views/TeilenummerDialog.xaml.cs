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
    public partial class TeileNummerDialog : Window
    {
        public string Teilenummer { get; private set; }

        public TeileNummerDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTeilenummer.Text))
            {
                Teilenummer = txtTeilenummer.Text.Trim();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Bitte eine Teilenummer eingeben.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
