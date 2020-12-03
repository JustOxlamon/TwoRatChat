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
using TwoRatChat.Model;

namespace TwoRatChat {
    /// <summary>
    /// Interaction logic for FortuneWindow.xaml
    /// </summary>
    public partial class FortuneWindow : Window {
        public FortuneWindow() {
            InitializeComponent();

            
        }

        FortuneList _fl;
        public void Show( FortuneList fl ) {
            fl.SecondsLeft = 60;
            fl.Pick = "hello";
            fl.WinnerCount = 1;

            this.DataContext = _fl = fl;
            this.Show();
        }

        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            this.Close();
        }

        private void StartRing( object sender, RoutedEventArgs e ) {
            _fl.Start();
        }

        private void StopRing( object sender, RoutedEventArgs e ) {
            _fl.Clear();
        }

        private void Thumb_DragDelta_1( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

    }
}
