using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Логика взаимодействия для ListSkinsDialog.xaml
    /// </summary>
    public partial class ListSkinsDialog : Window {
        public ListSkinsDialog() {
            Skins = new ObservableCollection<string>();
            InitializeComponent();
            this.DataContext = this;
        }

        public string SkinName {
            get { return (string)GetValue( SkinNameProperty ); }
            set { SetValue( SkinNameProperty, value ); }
        }

        // Using a DependencyProperty as the backing store for SkinName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SkinNameProperty =
            DependencyProperty.Register( "SkinName", typeof( string ), typeof( ListSkinsDialog ), new PropertyMetadata( "" ) );

        public ObservableCollection<string> Skins { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Dictionary<string, string> d = new Dictionary<string, string>();

            foreach ( var file in Directory.GetFiles( App.DataFolder, "*.htm?" ) ) 
                d[System.IO.Path.GetFileName( file )] = file;

            foreach ( var file in Directory.GetFiles( App.DataLocalFolder, "*.htm?" ) ) 
                d[System.IO.Path.GetFileName( file )] = file;

            foreach ( var kv in d )
                Skins.Add( kv.Key );
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            if ( string.IsNullOrEmpty( SkinName ) )
                return;

            this.DialogResult = true;
            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
