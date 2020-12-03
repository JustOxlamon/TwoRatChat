using Microsoft.Win32;
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
using TwoRatChat.Main.Designer;

namespace TwoRatChat.Main.Dialogs {
    /// <summary>
    /// Логика взаимодействия для SkinDesigner.xaml
    /// </summary>
    public partial class SkinDesigner : Window {
        public SkinDesigner() {
            InitializeComponent();
        }

        DesignDocument _document = null;

        private void NewDesign_Click(object sender, RoutedEventArgs e) {
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "TwoRatChat templates|*.trct";
            //ofd.InitialDirectory = App.DataFolder + "\\templates";
            //var ret = ofd.ShowDialog();

            //if( ret.HasValue && ret.Value ) {

            //}

            SelectTemplate st = new SelectTemplate();
            var ret = st.ShowDialog();
            if ( ret.HasValue && ret.Value ) {
                _document = new DesignDocument();
                if ( _document.CreateNew( st.FileName ) ) {

                    rebuildEditors();
                } else {
                    MessageBox.Show( "Template load error!" );
                }
            }
        }

        private void rebuildEditors() {
            editorRoot.Children.Clear();

            string group = "";

            foreach( var editor in from g in _document.Editors 
                                   orderby g.Group
                                   select g ) {

                if( group != editor.Group ) {
                    TextBlock tb = new TextBlock() { Text = editor.Group, FontSize = 20.0, Margin = new Thickness( 1, 10, 1, 1 ) };
                    editorRoot.Children.Add( tb );
                    group = editor.Group;
                }

                var xxx = editor.CreateControl();
                editorRoot.Children.Add( xxx );
            }

        }

        private void SaveDesign_Click(object sender, RoutedEventArgs e) {
            if ( _document == null )
                return;

            if ( string.IsNullOrEmpty( _document.FileName ) )
                SaveAsDesign_Click( sender, e );
            else
                _document.Save();
        }

        private void SaveAsDesign_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "TwoRatChat skins|*.html;*.htm";
            sfd.InitialDirectory = App.DataLocalFolder;

            var ret = sfd.ShowDialog();
            if ( ret.HasValue && ret.Value ) {

                //string p = System.IO.Path.GetDirectoryName( sfd.FileName );
                //if ( p != sfd.InitialDirectory ) {
                //    MessageBox.Show( "Please, do not change initial directory!" );
                //    goto again;
                //}

                _document.FileName = sfd.FileName;
                _document.Save();
            }
        }

        private void OpenDesign_Click(object sender, RoutedEventArgs e) {
            ListSkinsDialog lsd = new ListSkinsDialog();
            var ret = lsd.ShowDialog();
            if ( ret.HasValue && ret.Value ) {
                _document = new DesignDocument();
                if ( _document.Open( App.MapFileName( "\\" + lsd.SkinName ) ) ) {
                    rebuildEditors();
                } else {
                    editorRoot.Children.Clear();
                    MessageBox.Show( "Сохранение не принадлежит шаблону и изменено быть не может." );
                }
            }
        }

        private void UpdateDesign_Click(object sender, RoutedEventArgs e) {
            SaveDesign_Click( sender, e );
            MainWindow.ReloadSkin();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            if ( _document != null ) {
                if ( !string.IsNullOrEmpty( _document.FileName ) ) {
                    string f = _document.FileName;
                    _document.Save();
                    _document = new DesignDocument();
                    _document.Open( f );
                    rebuildEditors();
                    _document.Save();
                }
            }

            MainWindow.ReloadSkin();
        }
    }
}
