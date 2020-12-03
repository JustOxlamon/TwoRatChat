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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwoRatChat.Main.Designer.Controls {
    /// <summary>
    /// Логика взаимодействия для BorderPropEditor.xaml
    /// </summary>
    public partial class BorderPropEditor : UserControl {
        public BorderPropEditor() {
            InitializeComponent();
        }

        MarginPropertyEditor _editor;

        public void Setup(MarginPropertyEditor editor) {
            this._editor = editor;
            this.DataContext = this._editor;
            this.Caption.Text = _editor.Caption;
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e) {
          
        }
    }
}
