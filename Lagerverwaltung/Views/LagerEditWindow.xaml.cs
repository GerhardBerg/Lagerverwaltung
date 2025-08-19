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
    /// Interaktionslogik für LagerEditWindow.xaml
    /// </summary>
    public partial class LagerEditWindow : Window
    {
        private LagerItem _item;
        public LagerEditWindow(LagerItem item)
        {
            InitializeComponent();
            _item = item;
            DataContext = _item;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // signalisiert Änderungen übernommen
            Close();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // signalisiert Änderungen abgebrochen
            Close();
        }
    }
}
