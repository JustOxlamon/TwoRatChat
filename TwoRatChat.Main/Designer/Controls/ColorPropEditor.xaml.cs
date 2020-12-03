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
    /// Логика взаимодействия для ColorPropEditor.xaml
    /// </summary>
    public partial class ColorPropEditor : UserControl {
        public ColorPropEditor() {
            InitializeComponent();
        }

        ColorPropertyEditor _editor;
        public void Setup( ColorPropertyEditor editor) {
            this._editor = editor;
            this.CurrentColor.Color = _editor.CurrentColor;
            this.Caption.Text = editor.Caption;
            this.ToolTip = editor.Tooltip;
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e) {
            this._editor.CurrentColor = this._editor.DefaultColor;
            this.CurrentColor.Color = this._editor.DefaultColor;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e) {
            WPFColorPickerLib.ColorDialog cd = new WPFColorPickerLib.ColorDialog( this._editor.CurrentColor );
            var r = cd.ShowDialog();
            if ( r.HasValue && r.Value ) {
                this._editor.CurrentColor = cd.SelectedColor;
                this.CurrentColor.Color = cd.SelectedColor;
            }
        }
    }
}
