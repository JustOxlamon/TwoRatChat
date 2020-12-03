using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window {
        public OptionsDialog() {
            InitializeComponent();

            this.about.Text = string.Format( "TwoRatChat [ver. {0}] by Oxlamon", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version );
            _GGMode.Add( new IdValue( 0, "Full mode: "+ Properties.Resources.tip_goodGameTooltip ) );
            _GGMode.Add( new IdValue( 1, "Viewers (users in chat)" ) );
            _GGMode.Add( new IdValue( 2, "Viewers" ) );
            _GGMode.Add( new IdValue( 3, "Users in chat" ) );
            _GGMode.Add( new IdValue( 4, "Nothing" ) );

            voices.ItemsSource = TwoRatChat.Commands.CommandFactory.GetVoices;

            ggmode.ItemsSource = _GGMode;

            _ImageMode.Add( new IdValue( 0, "Do not show" ) );
            _ImageMode.Add( new IdValue( 3, "Only from whitelist" ) );
            _ImageMode.Add( new IdValue( 4, "All but, not from blacklist" ) );
            _ImageMode.Add( new IdValue( 5, "All (not recomended)" ) );

            imagemode.ItemsSource = _ImageMode;
            //ggmode
            //Skins
            List<string> html = new List<string>();
            foreach (var ld in Directory.GetFiles( App.DataLocalFolder, "*.html", SearchOption.AllDirectories ))
                html.Add( ld.Substring( App.DataLocalFolder.Length ) );
            foreach (var ld in Directory.GetFiles( App.DataFolder, "*.html", SearchOption.AllDirectories ))
                html.Add( ld.Substring( App.DataFolder.Length ) );

            HashSet<string> h = new HashSet<string>();
            foreach (var x in html) {
                string id = App.MapFileName( x );
                if (!string.IsNullOrEmpty( id ) && !h.Contains( x ))
                    h.Add( x.Replace("\\", "/") );
            }

            foreach (var m in h)
                Skins.Items.Add( m );

            Skins.SelectedItem = Properties.Settings.Default.Chat_RootSkin;
        }

        List<IdValue> _GGMode = new List<IdValue>();
        List<IdValue> _ImageMode = new List<IdValue>();

        private void Rectangle_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {
            WPFColorPickerLib.ColorDialog cd = new WPFColorPickerLib.ColorDialog( Converters.ColorToUIntConverter.FromUInt( Properties.Settings.Default.Window_AccentColor ) );
            var r = cd.ShowDialog();
            if( r.HasValue && r.Value )
                Properties.Settings.Default.Window_AccentColor = Converters.ColorToUIntConverter.FromColor( cd.SelectedColor );
        }

        private void Rectangle_MouseLeftButtonDown2( object sender, MouseButtonEventArgs e ) {
            WPFColorPickerLib.ColorDialog cd = new WPFColorPickerLib.ColorDialog( Converters.ColorToUIntConverter.FromUInt( Properties.Settings.Default.Window_BackgroundColor ) );
            var r = cd.ShowDialog();
            if (r.HasValue && r.Value)
                Properties.Settings.Default.Window_BackgroundColor = Converters.ColorToUIntConverter.FromColor( cd.SelectedColor );
        }

        private void ApplySkin_Click( object sender, RoutedEventArgs e ) {
            MainWindow.ReloadSkin();
        }

        private void SelectFileName_Click( object sender, RoutedEventArgs e ) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "png files|*.png";
            sfd.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.CommonDocuments );
            bool? ret = sfd.ShowDialog();
            if (ret.HasValue && ret.Value) {
                Properties.Settings.Default.Chat_ImagePath = sfd.FileName;
            }
        }

        private void EditHttpUri_Click(object sender, RoutedEventArgs e) {
            HttpListenEditor hle = new HttpListenEditor();
            hle.ShowDialog();
        }

        public class IdValue {
            public int id { get; set; }
            public string text { get; set; }

            public IdValue( int id, string text ) {
                this.id = id;
                this.text = text;
            }

            public override string ToString() {
                return text;
            }
        }

        private void ButtonWL_Click( object sender, RoutedEventArgs e ) {
            BlackListForm blf = new BlackListForm();
            blf.ShowDialog( MainWindow.chat.ImageWhiteList, "Image white list" );
        }

        private void ButtonBL_Click( object sender, RoutedEventArgs e ) {
            BlackListForm blf = new BlackListForm();
            blf.ShowDialog( MainWindow.chat.ImageBlackList, "Image black list" );

        }
    }
}
