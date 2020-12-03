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

namespace TwoRatChat.Launcher {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        public void SetUpdateText( string whatsNews ) {
            News.Text = whatsNews;
        }

        private void Thumb_DragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            this.DialogResult = false;
        }

        private void Button_Download_Click( object sender, RoutedEventArgs e ) {
            this.DialogResult = true;
        }


    }
}
