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

namespace TwoRatChat.Main {
    /// <summary>
    /// Interaction logic for Talk.xaml
    /// </summary>
    public partial class Talk : Window {
        public Talk() {
            InitializeComponent();

            talkText01.ItemsSource = TwoRatChat.Commands.CommandFactory.GetVoices;

            //talkText01
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            if ( !string.IsNullOrEmpty( talkText00.Text ) )
                TwoRatChat.Commands.CommandFactory.Talk( talkText01.Text, talkText00.Text );
        }

        private void talkText00_KeyDown( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Return ) {
                if( !string.IsNullOrEmpty( talkText00.Text ) )
                    TwoRatChat.Commands.CommandFactory.Talk( talkText01.Text, talkText00.Text );
            }
        }
    }
}
