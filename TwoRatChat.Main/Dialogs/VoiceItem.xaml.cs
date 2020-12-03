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

namespace TwoRatChat.Main.Dialogs {
    /// <summary>
    /// Interaction logic for VoiceItem.xaml
    /// </summary>
    public partial class VoiceItem : UserControl {
        public VoiceItem() {
            InitializeComponent();
            talkText01.ItemsSource = TwoRatChat.Commands.CommandFactory.GetVoices;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            if( !string.IsNullOrEmpty( talkText00.Text ) )
                TwoRatChat.Commands.CommandFactory.Talk( talkText01.Text, talkText00.Text );
        }

        private void talkText00_KeyDown( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Return ) {
                if( !string.IsNullOrEmpty( talkText00.Text ) )
                    TwoRatChat.Commands.CommandFactory.Talk( talkText01.Text, talkText00.Text );
            }
        }

        public static DependencyProperty CurItemProperty =
             DependencyProperty.Register( "CurItem", typeof( string ), typeof( VoiceItem ), new UIPropertyMetadata( String.Empty, ( a, b ) => {
                 string[] xx = (b.NewValue as string)?.Split( '|' );
                 if( xx != null && xx.Length >= 2 ) {
                     VoiceItem vi = a as VoiceItem;
                     vi.talkText00.Text = xx[0];
                     vi.talkText01.SelectedItem = xx[1];
                 }
             } ) );

        public string CurItem {
            get { return (string)GetValue( CurItemProperty ); }
            set { SetValue( CurItemProperty, value ); }
        }
    }
}
