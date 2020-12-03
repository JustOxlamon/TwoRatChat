// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TwoRatChat.Model {
    public enum PollStatusEnum {
        Unknown,
        Started,
        Finished
    }

    public class PollChoice: INotifyPropertyChanged {
       

        private int _value = 0;
        private string _Caption = string.Empty;
        public string Id { get; set; }
        public string Caption {
            get { return _Caption; }
            set {
                if( _Caption != value ) {
                    _Caption = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Caption" ) );
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "FullCaption" ) );
                }
            }
        }

        public string FullCaption {
            get { return string.Format("'{2}' = {0} ({1})", _Caption, Value, Id); }
        }

        public int Value {
            get { return _value; }
            set {
                if( _value != value ) {
                    _value = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Value" ) );
                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "FullCaption" ) );
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PollChoice() {
            this.Id = "1.";
            this.Caption = "pika";
        }
    }

    public class Polling : ObservableCollection<PollChoice>, INotifyPropertyChanged {
        int _secondsLeft = 0;
        string _Title = "";
        bool _AllowChangeChoice = false;
        PollStatusEnum _Started = PollStatusEnum.Unknown;
        DispatcherTimer _Timer;
        readonly Dictionary<string, PollChoice> _selectedChoices = new Dictionary<string, PollChoice>();

        /// <summary>
        /// Разрешить удалять свой голос в процессе голосования
        /// </summary>
        public bool AllowRemoveChoice { get; set; }
        /// <summary>
        /// Разрешить менять свой голос в процессе голосования
        /// </summary>
        public bool AllowChangeChoice {
            get { return _AllowChangeChoice; }
            set {
                if( _AllowChangeChoice != value ) {
                    _AllowChangeChoice = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "AllowChangeChoice" ) );
                }
            }
        }


        public string Title {
            get { return _Title; }
            set {
                if( _Title != value ) {
                    _Title = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Title" ) );
                }
            }
        }

        public PollStatusEnum Started {
            get { return _Started; }
            set {
                if( _Started != value ) {
                    _Started = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "Started" ) );
                }
            }
        }
        /// <summary>
        /// Кол-во секунд до конца голосования
        /// </summary>
        public int SecondsLeft {
            get { return _secondsLeft; }
            set {
                if( _secondsLeft != value ) {
                    _secondsLeft = value;

                    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( "SecondsLeft" ) );
                }
            }
        }

        public Polling() {
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Tick += _Timer_Tick;
            _Timer.Start();
        }

        void _Timer_Tick( object sender, EventArgs e ) {
            if (this.Started == PollStatusEnum.Started) {
                if (this.SecondsLeft > 0)
                    this.SecondsLeft--;
                else {
                    this.Started = PollStatusEnum.Finished;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Winner"));
                }
            }
        }


        public new event PropertyChangedEventHandler PropertyChanged;

        private PollChoice getChoice( ChatMessage message ) {
            foreach (var v in this)
                if (!string.IsNullOrEmpty(v.Id))
                    if (message.Text.Contains(v.Id))
                        return v;
            return null;
        }

        public void OnMessage( ChatMessage message ) {
            if ( this._Started == PollStatusEnum.Started ) {
                string user = message.Name.ToLower();
                PollChoice newPc = null;
                PollChoice oldPc;

                if (_selectedChoices.TryGetValue(user, out oldPc)) {
                    if (AllowChangeChoice) {
                        newPc = getChoice(message);
                        if (newPc != null || AllowRemoveChoice) {
                            oldPc.Value--;
                            _selectedChoices.Remove(user);

                            if (PropertyChanged != null)
                                PropertyChanged(this, new PropertyChangedEventArgs("Winner"));
                        }
                    }
                } else {
                    newPc = getChoice(message);
                }

                if (newPc != null) {
                    newPc.Value++;
                    _selectedChoices[user] = newPc;
                    Console.WriteLine( "{0} -> {1}", user, newPc.FullCaption );
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Winner"));
                }
            }
        }

        public PollChoice Winner {
            get {
                PollChoice pc = null;
                foreach (var p in this)
                    if (pc == null || pc.Value < p.Value)
                        pc = p;
                return pc;
            }
        }

        public void Start() {
            this.Started = PollStatusEnum.Started;
            this._selectedChoices.Clear();
            foreach (var p in this)
                p.Value = 0;
        }

        public PollChoice End() {
            this.Started = PollStatusEnum.Unknown;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Winner"));
            return Winner;
        }

        public new void Clear() {
            this.Title = "";
            this.Started = PollStatusEnum.Unknown;
            this.SecondsLeft = 0;
            this._selectedChoices.Clear();
            base.Clear();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Winner"));
        }

        public void Load( string fileName ) {
            try {
                Clear();
                string[] lines = File.ReadAllLines(fileName);
                Title = lines[0];

                for (int j = 1; j < lines.Length; ++j) {
                    string[] data = lines[j].Split(';');
                    Add(new PollChoice() {
                        Id = data[0],
                        Caption = data[1]
                    });
                }
            } catch {
                Clear();
            }
        }

        public void Save( string fileName ) {
            try {
                List<string> _lines = new List<string>();
                _lines.Add(_Title);

                foreach (var item in this)
                    _lines.Add(string.Format("{0};{1}", item.Id, item.Caption));

                File.WriteAllLines(fileName, _lines.ToArray());
            } catch {
            }
        }
    }
}
