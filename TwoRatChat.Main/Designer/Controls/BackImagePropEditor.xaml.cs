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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwoRatChat.Main.Designer.Controls {
    /// <summary>
    /// Логика взаимодействия для BackImagePropEditor.xaml
    /// </summary>
    public partial class BackImagePropEditor : UserControl {
        public BackImagePropEditor() {
            InitializeComponent();
        }

        BackgroundPropertyEditor _editor;

        public void Setup(BackgroundPropertyEditor bpr) {
            _editor = bpr;
            this.DataContext = _editor;
            Caption.Text = _editor.Caption;

            UpdateTooltips();
        }

        private void UpdateTooltips() {
            b1.ToolTip = new Image() { Source = _editor.Image1, MaxHeight = 200, MaxWidth = 200 };
            b2.ToolTip = new Image() { Source = _editor.Image2, MaxHeight = 200, MaxWidth = 200 };
        }

        private void SelectColor_Click(object sender, RoutedEventArgs e) {
          
        }

        private void ResetToDefault_Click(object sender, RoutedEventArgs e) {
         
        }

        private void SelectColor1_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.png;*.gif;*.jpg;*.jpeg";
            ofd.InitialDirectory = App.DataFolder + "\\img";
            var ret = ofd.ShowDialog();

            if ( ret.HasValue && ret.Value ) {
                _editor.SetBackground( ofd.FileName );
                UpdateTooltips();
            }
        }

        private void SelectColor2_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.png;*.gif;*.jpg;*.jpeg";
            ofd.InitialDirectory = App.DataFolder + "\\img";
            var ret = ofd.ShowDialog();

            if ( ret.HasValue && ret.Value ) {
                _editor.SetBorder( ofd.FileName );
                UpdateTooltips();
            }
        }

        private void b2_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if( MessageBox.Show( "Удалить картинку?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes ) {
                _editor.SetBorder( "" );
                UpdateTooltips();
            }
        }

        private void b1_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if ( MessageBox.Show( "Удалить картинку?", "Внимание", MessageBoxButton.YesNo ) == MessageBoxResult.Yes ) {
                _editor.SetBackground( "" );
                UpdateTooltips();
            }
        }
    }
}
