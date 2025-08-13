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
    /// Interaktionslogik für Lager.xaml
    /// </summary>
    public partial class Lager : Window
    {
        public Lager()
        {
            InitializeComponent();
            //dgLager.ItemsSource = DatabaseHelper
            //    .ExecuteQuery("SELECT * FROM Artikel")
            //    .DefaultView;
        }
    }
}
