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

namespace TwoRatChat.Main.Designer.Controls {
    /// <summary>
    /// Логика взаимодействия для EnumPropEditor.xaml
    /// </summary>
    public partial class EnumPropEditor : UserControl {
        public EnumPropEditor() {
            InitializeComponent();
        }

        EnumPropertyEditor _editor;
        public void Setup(EnumPropertyEditor editor) {
            this._editor = editor;
            this.Caption.Text = editor.Caption;
            this.ToolTip = editor.Tooltip;

            //this._editor.
            foreach( var kv in this._editor.Variants ) {
                string[] kk = kv.Split( '¦' );

                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = kk[0];
                if ( kk.Length < 2 )
                    cbi.Tag = kk[0];
                else
                    cbi.Tag = kk[1];

                EnumCB.Items.Add( cbi );

                if ( kk.Length < 2 ) {
                    if ( this._editor.CurrentValue == kk[0] )
                        EnumCB.SelectedItem = cbi;
                } else {
                    if ( this._editor.CurrentValue == kk[1] )
                        EnumCB.SelectedItem = cbi;
                }
            }
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e) {
            _editor.CurrentValue = _editor.DefaultValue;
            EnumCB.SelectedItem = null;
            foreach ( ComboBoxItem c in EnumCB.Items )
                if ( (c.Tag as string) == _editor.CurrentValue ) {
                    EnumCB.SelectedItem = c;
                    break;
                }
        }

        private void EnumCB_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem cbi = EnumCB.SelectedItem as ComboBoxItem;
            if( cbi != null ) {
                _editor.CurrentValue = cbi.Tag as string;
            }
        }
    }
}
