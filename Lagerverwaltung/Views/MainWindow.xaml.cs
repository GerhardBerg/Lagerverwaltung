using Lagerverwaltung.Views;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lagerverwaltung.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Bestellungen_Click(object sender, RoutedEventArgs e)
        {
            new Bestellungen().ShowDialog();
        }

        private void Wareneingang_Click(object sender, RoutedEventArgs e)
        {
            new Wareneingang().ShowDialog();
        }

        private void Lager_Click(object sender, RoutedEventArgs e)
        {
            new Lager().ShowDialog();
        }

        private void Warenausgang_Click(object sender, RoutedEventArgs e)
        {
            new Warenausgang().ShowDialog();
        }
    }
}
