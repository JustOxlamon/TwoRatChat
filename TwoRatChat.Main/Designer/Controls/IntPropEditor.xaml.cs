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
    /// Логика взаимодействия для IntPropEditor.xaml
    /// </summary>
    public partial class IntPropEditor : UserControl {
        public IntPropEditor() {
            InitializeComponent();
        }

        IntPropertyEditor _editor;
        public void Setup(IntPropertyEditor editor) {
            this._editor = editor;
            this.intSlider.Minimum = editor.Min;
            this.intSlider.Maximum = editor.Max;
            this.intSlider.Value = editor.Current;

            this.Caption.Text = editor.Caption;
            this.ToolTip = editor.Tooltip;

            this.cnt.Text = string.Format( "({0})", _editor.Current );
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e) {
            this.intSlider.Value = _editor.Current = _editor.Default;
        }

        private void intSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            _editor.Current = (int)intSlider.Value;
            this.cnt.Text = string.Format( "({0})", _editor.Current );
        }

     
    }
}
