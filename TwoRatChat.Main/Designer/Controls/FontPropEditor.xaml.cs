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
    /// Логика взаимодействия для FontPropEditor.xaml
    /// </summary>
    public partial class FontPropEditor : UserControl {
        public FontPropEditor() {
            InitializeComponent();
        }

        FontPropertyEditor _editor;

        private void ResetToDefault_Click(object sender, RoutedEventArgs e) {
            this.EnumCB.SelectedItem = new FontFamily( this._editor.DefaultFontFamily );
        }

        internal void Setup(FontPropertyEditor fontPropertyEditor) {
            _editor = fontPropertyEditor;
            this.Caption.Text = _editor.Caption;
            this.DataContext = _editor;

            this.EnumCB.SelectedItem = new FontFamily( this._editor.FontFamily );
        }

        private void EnumCB_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this._editor.FontFamily = (EnumCB.SelectedItem as FontFamily).Source;
        }
    }
}
