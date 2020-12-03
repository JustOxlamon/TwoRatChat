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
    /// Логика взаимодействия для Downloading.xaml
    /// </summary>
    public partial class Downloading : Window {
        public Downloading() {
            InitializeComponent();
        }

        internal void OnProgress( string title, double? progress ) {
            this.progressTitle.Text = title;
            if (progress.HasValue) {
                this.progress.IsIndeterminate = false;
                this.progress.Value = progress.Value;
            } else {
                this.progress.IsIndeterminate = true;
            }
        }

        private void Thumb_DragDelta( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            Close();
        }
    }
}
