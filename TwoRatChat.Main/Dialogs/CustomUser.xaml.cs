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

namespace TwoRatChat.Main.Dialogs {
    /// <summary>
    /// Interaction logic for CustomUser.xaml
    /// </summary>
    public partial class CustomUser : Window {
        public CustomUser() {
            InitializeComponent();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            sourceColumn.ItemsSource = Sources.SourceManager.Sources;
            voiceColumn.ItemsSource = TwoRatChat.Commands.CommandFactory.GetVoices;
            System.Windows.Data.CollectionViewSource customUsersViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource( "customUsersViewSource" )));
            // Load data by setting the CollectionViewSource.Source property:
            customUsersViewSource.Source = MainWindow.chat.CustomUsers;
        }

        private void Button_Click( object sender, RoutedEventArgs e ) {
            MainWindow.chat.CustomUsers.Add( new Model.CustomUserItem() );
        }

        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            var x = customUsersDataGrid.SelectedItem as TwoRatChat.Main.Model.CustomUserItem;
            if( x != null )
                MainWindow.chat.CustomUsers.Remove( x );
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e ) {
            MainWindow.chat.CustomUsers.Save();
        }
    }
}
