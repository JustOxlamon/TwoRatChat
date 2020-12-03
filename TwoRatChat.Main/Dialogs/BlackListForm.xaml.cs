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
    /// Interaction logic for BlackListForm.xaml
    /// </summary>
    public partial class BlackListForm : Window {
        public BlackListForm() {
            InitializeComponent();

        }


        BlackList _blackList;

        private void Thumb_DragDelta_1( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        public void ShowDialog( BlackList blackList, string caption = "Message black list" ) {
            bl.DataContext = _blackList = blackList;
            this.Title = caption;
            base.ShowDialog();
        }

        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            this.Close();
        }

        private void Button_Click_2( object sender, RoutedEventArgs e ) {
            BlackItem bi = (sender as Button).Tag as BlackItem;
            _blackList.Remove(bi);
        }

        private void Button_Click_3( object sender, RoutedEventArgs e ) {
            BlackItem bi = new BlackItem(-1, "nickname", "replace text");
            _blackList.Add(bi);
        }
    }
}
