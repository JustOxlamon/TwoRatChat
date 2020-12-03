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
using TwoRatChat.Model;

namespace TwoRatChat.Main {
    /// <summary>
    /// Interaction logic for PollingEditorForm.xaml
    /// </summary>
    public partial class PollingEditorForm : Window {
        public PollingEditorForm() {
            InitializeComponent();
        }

        Polling _polling;

        public void Show( Polling polling ) {
            this.DataContext = _polling = polling;
            this.Show();
        }

        private void Button_Click_1( object sender, RoutedEventArgs e ) {
            this.Close();
        }

        private void Button_Click_2( object sender, RoutedEventArgs e ) {
            PollChoice pc = (sender as Button).Tag as PollChoice;
            _polling.Remove(pc);
        }

        private void Button_Click_3( object sender, RoutedEventArgs e ) {
            _polling.Add(new PollChoice());
        }

        private void Thumb_DragDelta_1( object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e ) {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        private void StartPoll( object sender, RoutedEventArgs e ) {
            this._polling.Start();
        }

        private void StopPoll( object sender, RoutedEventArgs e ) {
            this._polling.End();
        }

        private void ClearAll( object sender, RoutedEventArgs e ) {
            this._polling.Clear();
        }

        private void SavePoll( object sender, RoutedEventArgs e ) {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "text files|*.txt";
            sfd.FileName = "polling.txt";
            var r = sfd.ShowDialog();
            if (r.HasValue && r.Value)
                this._polling.Save(sfd.FileName);
        }

        private void LoadPoll( object sender, RoutedEventArgs e ) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "text files|*.txt";
            var r = ofd.ShowDialog();
            if (r.HasValue && r.Value)
                this._polling.Load(ofd.FileName);
        }
    }
}
