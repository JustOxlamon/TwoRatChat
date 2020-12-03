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

namespace TwoRatChat.Launcher {
    /// <summary>
    /// Логика взаимодействия для InputString.xaml
    /// </summary>
    public partial class InputString : Window {
        public InputString() {
            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            this.Text = this.text.Text;
            this.DialogResult = true;
        }

        public string Text;
    }
}
