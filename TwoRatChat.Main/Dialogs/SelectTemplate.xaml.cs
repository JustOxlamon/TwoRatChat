using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для SelectTemplate.xaml
    /// </summary>
    public partial class SelectTemplate : Window {
        public SelectTemplate() {
            InitializeComponent();
            Loaded += SelectTemplate_Loaded;
        }

        public List<TemplateItem> Templates { get; set; }
        public TemplateItem SelectedTemplate { get; set; }

        public class TemplateItem {
            public string Caption { get; set; }
            public string Desc { get; set; }
            public string FileName { get; set; }

            public string Id { get; set; }

            public override string ToString() {
                return Caption;
            }
        }

        private void SelectTemplate_Loaded(object sender, RoutedEventArgs e) {
            Dictionary<string, TemplateItem> items = new Dictionary<string, TemplateItem>();
            foreach( var file in Directory.GetFiles( App.DataFolder + "\\templates" ) ) {
                TemplateItem item = loadTemplate( file );
                items[item.Id] = item;
            }

            if( Directory.Exists( App.DataLocalFolder + "\\templates" ) )
            foreach ( var file in Directory.GetFiles( App.DataLocalFolder + "\\templates" ) ) {
                TemplateItem item = loadTemplate( file );
                items[item.Id] = item;
            }

            Templates = new List<TemplateItem>();
            foreach ( var x in items )
                Templates.Add( x.Value );

            tmpl.DataContext = this;
        }

        private TemplateItem loadTemplate(string file) {
            string x = File.ReadAllText( file );
            TemplateItem ti = new TemplateItem();

            ti.Id = System.IO.Path.GetFileName( file );
            ti.FileName = file;

            Regex r = new Regex( "<!--%(.*?)=(.*?)%-->" );
            foreach( Match m in r.Matches( x ) ) {
                switch( m.Groups[1].Value ) {
                    case "NAME":
                        ti.Caption = m.Groups[2].Value;
                        break;
                    case "DESC":
                        ti.Desc = m.Groups[2].Value;
                        break;
                }
            }

            return ti;
        }

        public string FileName {
            get {
                if ( SelectedTemplate == null )
                    return null;
                return SelectedTemplate.FileName;
            }
        }

        private void tmpl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if ( SelectedTemplate != null )
                desc.Text = SelectedTemplate.Desc;
            else
                desc.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if ( SelectedTemplate != null )
                this.DialogResult = true;
            else
                MessageBox.Show( "Please, select template." );
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
