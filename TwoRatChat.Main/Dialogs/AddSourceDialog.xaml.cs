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
using TwoRatChat.Main.Sources;

namespace TwoRatChat {
    /// <summary>
    /// Interaction logic for AddSourceDialog.xaml
    /// </summary>
    public partial class AddSourceDialog : Window {
        public AddSourceDialog() {
            InitializeComponent();
        }

        private void okButton_Click( object sender, RoutedEventArgs e ) {
            if ( string.IsNullOrEmpty( chatSourceUri.Text.Trim() ) ) {
                chatSourceUri.Focus();
                return;
            }
            this.DialogResult = true;
        }

        private void cancelButton_Click( object sender, RoutedEventArgs e ) {
            this.DialogResult = false;
        }

        private void chatSourceCB_SelectionChanged_1( object sender, SelectionChangedEventArgs e ) {
        }

        private void Window_Loaded_1( object sender, RoutedEventArgs e ) {
            chatSourceCB.DataContext = SourceManager.Sources;
            chatSourceCB.SelectedIndex = 0;
        }
    }
}
