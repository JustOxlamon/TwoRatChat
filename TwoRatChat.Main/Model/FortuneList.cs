// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TwoRatChat.Model {
    public enum FortuneStatusEnum {
        Unknown,
        Started,
        Finished,
        ShowWinners
    }

    public class FortuneList:ObservableCollection<string>, INotifyPropertyChanged {
        string _Title = "";
        string _Pick = "";
        int _WinnerCount = 1;
        int _secondsLeft = 0;
        Random rnd = new Random();
        FortuneStatusEnum _Status = FortuneStatusEnum.Unknown;
        DispatcherTimer _Timer;

        // TODO: Fix CS0114
        public new event PropertyChangedEventHandler PropertyChanged;

        public FortuneList() {
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();
        }

        public new void Add( string nickName ) {
            if( this.Contains( nickName ) )
                return;
            base.Add( nickName );

            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Counts" ) );
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Chances" ) );
        }

        public void OnMessage( ChatMessage message ){
            if (Status == FortuneStatusEnum.Started) {
                if( message.Text.Contains( _Pick ) ) {
                    if( this.Contains( message.Name ) )
                        return;
                    base.Add( message.Name );
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Counts" ) );
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Chances" ) );
                }
            }
        }

        public int Counts {
            get { return this.Count; }
        }


        public float Chances {
            get {
                if (this.Count == 0)
                    return 100.0f;
                return ((float)_WinnerCount / (float)this.Count * 100.0f);
            }
        }
        public string Pick {
            get { return _Pick; }
            set {
                if (_Pick != value) {
                    _Pick = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Pick" ) );
                }
            }
        }
        public string Title {
            get { return _Title; }
            set {
                if (_Title != value) {
                    _Title = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Title" ) );
                }
            }
        }
        public FortuneStatusEnum Status {
            get { return _Status; }
            set {
                if (_Status != value) {
                    _Status = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Status" ) );
                }
            }
        }
        public int SecondsLeft {
            get { return _secondsLeft; }
            set {
                if (_secondsLeft != value) {
                    if (value < 0)
                        value = 0;
                    _secondsLeft = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "SecondsLeft" ) );
                }
            }
        }
        public int WinnerCount {
            get { return _WinnerCount; }
            set {
                if( _WinnerCount != value ) {
                    if( value < 1 )
                        value = 1;
                    _WinnerCount = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "WinnerCount" ) );
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Chances" ) );
                }
            }
        }

        void _Timer_Tick( object sender, EventArgs e ) {
            switch (this.Status) {
                case FortuneStatusEnum.Unknown:
                    break;

                case FortuneStatusEnum.Started:
                    if (this.SecondsLeft > 0)
                        this.SecondsLeft--;
                    else 
                        this.Status = FortuneStatusEnum.Finished;
                    break;

                case FortuneStatusEnum.Finished:
                    int c = this.Count / 10;
                    if (c < 1) c = 1;

                    for (int j = 0; j < c; ++j) 
                        if (WinnerCount < this.Count) {
                            RemoveAt(rnd.Next(this.Count));
                        } else {
                            this.Status = FortuneStatusEnum.ShowWinners;
                            break;
                        }
                    break;

                case FortuneStatusEnum.ShowWinners:
                    break;
            }
        }

        public void Start() {
            this.Status = FortuneStatusEnum.Started;
            base.Clear();
        }

        public new void Clear() {
            base.Clear();
            this.Status = FortuneStatusEnum.Unknown;
        }
    }
}
