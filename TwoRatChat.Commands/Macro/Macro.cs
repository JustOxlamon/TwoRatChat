using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwoRatChat.Commands {
    class Macro {
        readonly List<WinAPI.INPUT> INPUTS = new List<WinAPI.INPUT>();

        public Macro() {
        }

        public string Parse( string text ) {
            string[] lines = text.Split( new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries );
            string Errors = "";
            for( int j=0; j<lines.Length; ++j ) {
                string line = lines[j].Split( '#' )[0].Trim( '\r', '\n', ' ', '\t' );
                string[] cmd = line.Split( ' ' );
                switch( cmd[0].ToLower() ) {
                    case "up":
                    case "keyup":
                        AddKeyUp( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ), false );
                        break;

                    case "xup":
                    case "scankeyup":
                        AddKeyUp( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ), true );
                        break;

                    case "down":
                    case "keydown":
                        AddKeyDown( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ), false );
                        break;

                    case "xdown":
                    case "scankeydown":
                        AddKeyDown( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ), true );
                        break;

                    //case "window":
                    //    WindowTitle = cmd[1];
                    //    break;

                    case "press":
                        AddKeyPress( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ),
                            false,
                            int.Parse( cmd[2] ), int.Parse( cmd[3] ) );
                        break;

                    case "xpress":
                        AddKeyPress( (VirtualKeyCode)Enum.Parse( typeof( VirtualKeyCode ), cmd[1] ),
                            true,
                            int.Parse( cmd[2] ), int.Parse( cmd[3] ) );
                        break;

                    case "sleep":
                    case "delay":
                        AddSleep( int.Parse( cmd[1] ) );
                        break;
                }
            }
            return Errors;
        }

        static bool IsExtendedKey(VirtualKeyCode keyCode) {
            if ( keyCode == VirtualKeyCode.Menu ||
                keyCode == VirtualKeyCode.LMenu ||
                keyCode == VirtualKeyCode.RMenu ||
                keyCode == VirtualKeyCode.Control ||
                keyCode == VirtualKeyCode.RControlKey ||
                keyCode == VirtualKeyCode.Insert ||
                keyCode == VirtualKeyCode.Delete ||
                keyCode == VirtualKeyCode.Home ||
                keyCode == VirtualKeyCode.End ||
                keyCode == VirtualKeyCode.Prior ||
                keyCode == VirtualKeyCode.Next ||
                keyCode == VirtualKeyCode.Right ||
                keyCode == VirtualKeyCode.Up ||
                keyCode == VirtualKeyCode.Left ||
                keyCode == VirtualKeyCode.Down ||
                keyCode == VirtualKeyCode.NumLock ||
                keyCode == VirtualKeyCode.Cancel ||
                keyCode == VirtualKeyCode.Snapshot ||
                keyCode == VirtualKeyCode.Divide ) {
                return true;
            } else {
                return false;
            }
        }

        static VirtualScanCode convert(VirtualKeyCode key) {
            switch( key ) {
                case VirtualKeyCode.None: return 0;
                case VirtualKeyCode.LButton: return 0;
                case VirtualKeyCode.RButton: return 0;
                case VirtualKeyCode.Cancel: return 0;
                case VirtualKeyCode.MButton: return 0;
                case VirtualKeyCode.XButton1: return 0;
                case VirtualKeyCode.XButton2: return 0;
                case VirtualKeyCode.LineFeed: return 0;
                case VirtualKeyCode.Clear: return 0;
                case VirtualKeyCode.IMEConvert: return 0;
                case VirtualKeyCode.IMENonconvert: return 0;
                case VirtualKeyCode.IMEAccept: return 0;
                case VirtualKeyCode.IMEModeChange: return 0;
                case VirtualKeyCode.Select: return 0;
                case VirtualKeyCode.Print: return 0;
                case VirtualKeyCode.Execute: return 0;
                case VirtualKeyCode.Help: return 0;
                case VirtualKeyCode.Separator: return 0;

                case VirtualKeyCode.KanaMode: return VirtualScanCode.KANA;
                case VirtualKeyCode.JunjaMode: return VirtualScanCode.KANJI;
                case VirtualKeyCode.FinalMode: return 0;
                case VirtualKeyCode.HanjaMode: return 0;


                case VirtualKeyCode.ShiftKey: return VirtualScanCode.LSHIFT;
                case VirtualKeyCode.ControlKey: return VirtualScanCode.LCONTROL;
                case VirtualKeyCode.Menu: return VirtualScanCode.LMENU;

                case VirtualKeyCode.Back: return VirtualScanCode.BACK;
                case VirtualKeyCode.Tab: return VirtualScanCode.TAB;
                case VirtualKeyCode.Return: return VirtualScanCode.RETURN;
                case VirtualKeyCode.Pause: return VirtualScanCode.PAUSE;
                case VirtualKeyCode.Capital: return VirtualScanCode.CAPITAL;
                case VirtualKeyCode.Escape: return VirtualScanCode.Escape;
                case VirtualKeyCode.Space: return VirtualScanCode.SPACE;
                case VirtualKeyCode.Prior: return VirtualScanCode.PRIOR;
                case VirtualKeyCode.Next: return VirtualScanCode.NEXT;
                case VirtualKeyCode.End: return VirtualScanCode.END;
                case VirtualKeyCode.Home: return VirtualScanCode.HOME;
                case VirtualKeyCode.Left: return VirtualScanCode.LEFT;
                case VirtualKeyCode.Up: return VirtualScanCode.UP;
                case VirtualKeyCode.Right: return VirtualScanCode.RIGHT;
                case VirtualKeyCode.Down: return VirtualScanCode.DOWN;
                case VirtualKeyCode.Snapshot: return VirtualScanCode.SYSRQ;
                case VirtualKeyCode.Insert: return VirtualScanCode.INSERT;
                case VirtualKeyCode.Delete: return VirtualScanCode.DELETE;
                case VirtualKeyCode.D0: return VirtualScanCode.D0;
                case VirtualKeyCode.D1: return VirtualScanCode.D1;
                case VirtualKeyCode.D2: return VirtualScanCode.D2;
                case VirtualKeyCode.D3: return VirtualScanCode.D3;
                case VirtualKeyCode.D4: return VirtualScanCode.D4;
                case VirtualKeyCode.D5: return VirtualScanCode.D5;
                case VirtualKeyCode.D6: return VirtualScanCode.D6;
                case VirtualKeyCode.D7: return VirtualScanCode.D7;
                case VirtualKeyCode.D8: return VirtualScanCode.D8;
                case VirtualKeyCode.D9: return VirtualScanCode.D9;
                case VirtualKeyCode.A: return VirtualScanCode.A;
                case VirtualKeyCode.B: return VirtualScanCode.B;
                case VirtualKeyCode.C: return VirtualScanCode.C;
                case VirtualKeyCode.D: return VirtualScanCode.D;
                case VirtualKeyCode.E: return VirtualScanCode.E;
                case VirtualKeyCode.F: return VirtualScanCode.F;
                case VirtualKeyCode.G: return VirtualScanCode.G;
                case VirtualKeyCode.H: return VirtualScanCode.H;
                case VirtualKeyCode.I: return VirtualScanCode.I;
                case VirtualKeyCode.J: return VirtualScanCode.J;
                case VirtualKeyCode.K: return VirtualScanCode.K;
                case VirtualKeyCode.L: return VirtualScanCode.L;
                case VirtualKeyCode.M: return VirtualScanCode.M;
                case VirtualKeyCode.N: return VirtualScanCode.N;
                case VirtualKeyCode.O: return VirtualScanCode.O;
                case VirtualKeyCode.P: return VirtualScanCode.P;
                case VirtualKeyCode.Q: return VirtualScanCode.Q;
                case VirtualKeyCode.R: return VirtualScanCode.R;
                case VirtualKeyCode.S: return VirtualScanCode.S;
                case VirtualKeyCode.T: return VirtualScanCode.T;
                case VirtualKeyCode.U: return VirtualScanCode.U;
                case VirtualKeyCode.V: return VirtualScanCode.V;
                case VirtualKeyCode.W: return VirtualScanCode.W;
                case VirtualKeyCode.X: return VirtualScanCode.X;
                case VirtualKeyCode.Y: return VirtualScanCode.Y;
                case VirtualKeyCode.Z: return VirtualScanCode.Z;
                case VirtualKeyCode.LWin: return VirtualScanCode.LWIN;
                case VirtualKeyCode.RWin: return VirtualScanCode.RWIN;
                case VirtualKeyCode.Apps: return VirtualScanCode.APPS;
                case VirtualKeyCode.Sleep: return VirtualScanCode.SLEEP;
                case VirtualKeyCode.NumPad0: return VirtualScanCode.NUMPAD0;
                case VirtualKeyCode.NumPad1: return VirtualScanCode.NUMPAD1;
                case VirtualKeyCode.NumPad2: return VirtualScanCode.NUMPAD2;
                case VirtualKeyCode.NumPad3: return VirtualScanCode.NUMPAD3;
                case VirtualKeyCode.NumPad4: return VirtualScanCode.NUMPAD4;
                case VirtualKeyCode.NumPad5: return VirtualScanCode.NUMPAD5;
                case VirtualKeyCode.NumPad6: return VirtualScanCode.NUMPAD6;
                case VirtualKeyCode.NumPad7: return VirtualScanCode.NUMPAD7;
                case VirtualKeyCode.NumPad8: return VirtualScanCode.NUMPAD8;
                case VirtualKeyCode.NumPad9: return VirtualScanCode.NUMPAD9;
                case VirtualKeyCode.Multiply: return VirtualScanCode.MULTIPLY;
                case VirtualKeyCode.Add: return VirtualScanCode.ADD;
                case VirtualKeyCode.Subtract: return VirtualScanCode.SUBTRACT;
                case VirtualKeyCode.Decimal: return VirtualScanCode.DECIMAL;
                case VirtualKeyCode.Divide: return VirtualScanCode.DIVIDE;
                case VirtualKeyCode.F1: return VirtualScanCode.F1;
                case VirtualKeyCode.F2: return VirtualScanCode.F2;
                case VirtualKeyCode.F3: return VirtualScanCode.F3;
                case VirtualKeyCode.F4: return VirtualScanCode.F4;
                case VirtualKeyCode.F5: return VirtualScanCode.F5;
                case VirtualKeyCode.F6: return VirtualScanCode.F6;
                case VirtualKeyCode.F7: return VirtualScanCode.F7;
                case VirtualKeyCode.F8: return VirtualScanCode.F8;
                case VirtualKeyCode.F9: return VirtualScanCode.F9;
                case VirtualKeyCode.F10: return VirtualScanCode.F10;
                case VirtualKeyCode.F11: return VirtualScanCode.F11;
                case VirtualKeyCode.F12: return VirtualScanCode.F12;
                case VirtualKeyCode.F13: return VirtualScanCode.F13;
                case VirtualKeyCode.F14: return VirtualScanCode.F14;
                case VirtualKeyCode.F15: return VirtualScanCode.F15;
                case VirtualKeyCode.F16: return 0;
                case VirtualKeyCode.F17: return 0;
                case VirtualKeyCode.F18: return 0;
                case VirtualKeyCode.F19: return 0;
                case VirtualKeyCode.F20: return 0;
                case VirtualKeyCode.F21: return 0;
                case VirtualKeyCode.F22: return 0;
                case VirtualKeyCode.F23: return 0;
                case VirtualKeyCode.F24: return 0;
                case VirtualKeyCode.NumLock: return VirtualScanCode.NUMLOCK;
                case VirtualKeyCode.Scroll: return VirtualScanCode.SCROLL;
                case VirtualKeyCode.LShiftKey: return VirtualScanCode.LSHIFT;
                case VirtualKeyCode.RShiftKey: return VirtualScanCode.RSHIFT;
                case VirtualKeyCode.LControlKey: return VirtualScanCode.LCONTROL;
                case VirtualKeyCode.RControlKey: return VirtualScanCode.RCONTROL;
                case VirtualKeyCode.LMenu: return VirtualScanCode.LMENU;
                case VirtualKeyCode.RMenu: return VirtualScanCode.RMENU;
                case VirtualKeyCode.BrowserBack: return VirtualScanCode.WEBBACK;
                case VirtualKeyCode.BrowserForward: return VirtualScanCode.WEBFORWARD;
                case VirtualKeyCode.BrowserRefresh: return VirtualScanCode.WEBREFRESH;
                case VirtualKeyCode.BrowserStop: return VirtualScanCode.WEBSTOP;
                case VirtualKeyCode.BrowserSearch: return VirtualScanCode.WEBSEARCH;
                case VirtualKeyCode.BrowserFavorites: return VirtualScanCode.WEBFAVORITES;
                case VirtualKeyCode.BrowserHome: return VirtualScanCode.WEBHOME;
                case VirtualKeyCode.VolumeMute: return VirtualScanCode.MUTE;
                case VirtualKeyCode.VolumeDown: return VirtualScanCode.VOLUMEDOWN;
                case VirtualKeyCode.VolumeUp: return VirtualScanCode.VOLUMEUP;
                case VirtualKeyCode.MediaNextTrack: return VirtualScanCode.NEXTTRACK;
                case VirtualKeyCode.MediaPreviousTrack: return VirtualScanCode.PREVTRACK;
                case VirtualKeyCode.MediaStop: return VirtualScanCode.MEDIASTOP;
                case VirtualKeyCode.MediaPlayPause: return VirtualScanCode.PLAYPAUSE;

                case VirtualKeyCode.LaunchMail: return 0;
                case VirtualKeyCode.SelectMedia: return 0;
                case VirtualKeyCode.LaunchApplication1: return 0;
                case VirtualKeyCode.LaunchApplication2: return 0;
                case VirtualKeyCode.OemSemicolon: return 0;
                case VirtualKeyCode.Oemplus: return 0;
                case VirtualKeyCode.Oemcomma: return 0;
                case VirtualKeyCode.OemMinus: return 0;
                case VirtualKeyCode.OemPeriod: return 0;
                case VirtualKeyCode.OemQuestion: return 0;
                case VirtualKeyCode.Oemtilde: return 0;
                case VirtualKeyCode.OemOpenBrackets: return 0;
                case VirtualKeyCode.OemPipe: return 0;
                case VirtualKeyCode.OemCloseBrackets: return 0;
                case VirtualKeyCode.OemQuotes: return 0;
                case VirtualKeyCode.Oem8: return 0;
                case VirtualKeyCode.OemBackslash: return 0;
                case VirtualKeyCode.ProcessKey: return 0;
                case VirtualKeyCode.Packet: return 0;
                case VirtualKeyCode.Attn: return 0;
                case VirtualKeyCode.Crsel: return 0;
                case VirtualKeyCode.Exsel: return 0;
                case VirtualKeyCode.EraseEof: return 0;
                case VirtualKeyCode.Play: return 0;
                case VirtualKeyCode.Zoom: return 0;
                case VirtualKeyCode.NoName: return 0;
                case VirtualKeyCode.Pa1: return 0;
                case VirtualKeyCode.OemClear: return 0;
                case VirtualKeyCode.KeyCode: return 0;
                case VirtualKeyCode.Shift: return 0;
                case VirtualKeyCode.Control: return 0;
                case VirtualKeyCode.Alt: return 0;
            }
            return 0;
        }

        public Macro AddSleep(int ms) {
            var down =
                new WinAPI.INPUT {
                    Type = (UInt32)WinAPI.InputType.Sleep,
                    Data =
                            {
                                Keyboard =
                                    new WinAPI.KEYBDINPUT
                                        {
                                            KeyCode = 0,
                                            Scan = 0,
                                            Flags = (uint)ms,
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                            }
                };

            INPUTS.Add( down );
            return this;
        }

        public Macro AddKeyUp(VirtualKeyCode key, bool isScanLine) {
            if ( isScanLine ) {
                var up =
                   new WinAPI.INPUT {
                       Type = (UInt32)WinAPI.InputType.Keyboard,
                       Data =
                               {
                                Keyboard =
                                    new WinAPI.KEYBDINPUT
                                        {
                                            KeyCode = 0,
                                            Scan = (UInt16)convert(key),
                                            Flags = ((UInt32) (IsExtendedKey(key)
                                                                  ? WinAPI.KeyboardFlag.KeyUp | WinAPI.KeyboardFlag.ScanCode | WinAPI.KeyboardFlag.ExtendedKey
                                                                  : WinAPI.KeyboardFlag.KeyUp | WinAPI.KeyboardFlag.ScanCode))

                                                    ,
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                               }
                   };

                INPUTS.Add( up );
            } else {
                var up =
                    new WinAPI.INPUT {
                        Type = (UInt32)WinAPI.InputType.Keyboard,
                        Data =
                                {
                                Keyboard =
                                    new WinAPI.KEYBDINPUT
                                        {
                                            KeyCode = (UInt16) key,
                                            Scan = 0,
                                            Flags = ((UInt32) (IsExtendedKey(key)
                                                                  ? WinAPI.KeyboardFlag.KeyUp | WinAPI.KeyboardFlag.ExtendedKey
                                                                  : WinAPI.KeyboardFlag.KeyUp))

                                                    ,
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                                }
                    };

                INPUTS.Add( up );
            }
            return this;
        }

        public Macro AddKeyPress(VirtualKeyCode keyCode, bool isScanLine, int delayAfterDown = 0, int delayAfterUp = 0) {
            AddKeyDown( keyCode, isScanLine );
            if ( delayAfterDown > 0 )
                AddSleep( delayAfterDown );
            AddKeyUp( keyCode, isScanLine );
            if ( delayAfterUp > 0 )
                AddSleep( delayAfterUp );
            return this;
        }

        public Macro AddKeyDown(VirtualKeyCode key, bool isScanLine) {
            if ( isScanLine ) {

                var down =
                new WinAPI.INPUT {
                    Type = (UInt32)WinAPI.InputType.Keyboard,
                    Data =
                            {
                                Keyboard =
                                    new WinAPI.KEYBDINPUT
                                        {
                                            KeyCode = 0,
                                            Scan =  (UInt16)convert( key ),
                                            Flags = (UInt32) (IsExtendedKey(key)
                                                                  ? WinAPI.KeyboardFlag.ExtendedKey | WinAPI.KeyboardFlag.ScanCode
                                                                  : WinAPI.KeyboardFlag.ScanCode),
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                            }
                };

                INPUTS.Add( down );

            } else {
                var down =
               new WinAPI.INPUT {
                   Type = (UInt32)WinAPI.InputType.Keyboard,
                   Data =
                           {
                                Keyboard =
                                    new WinAPI.KEYBDINPUT
                                        {
                                            KeyCode = (UInt16) key,
                                            Scan = 0,
                                            Flags = (UInt32) (IsExtendedKey(key) ? WinAPI.KeyboardFlag.ExtendedKey : 0),
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                           }
               };

                INPUTS.Add( down );
            }

            return this;
        }

        public Macro Add(char character) {
            UInt16 scanCode = character;

            var down = new WinAPI.INPUT {
                Type = (UInt32)WinAPI.InputType.Keyboard,
                Data =
                                   {
                                       Keyboard =
                                           new WinAPI.KEYBDINPUT
                                               {
                                                   KeyCode = 0,
                                                   Scan = scanCode,
                                                   Flags = (UInt32)WinAPI.KeyboardFlag.Unicode,
                                                   Time = 0,
                                                   ExtraInfo = IntPtr.Zero
                                               }
                                   }
            };

            var up = new WinAPI.INPUT {
                Type = (UInt32)WinAPI.InputType.Keyboard,
                Data =
                                 {
                                     Keyboard =
                                         new WinAPI.KEYBDINPUT
                                             {
                                                 KeyCode = 0,
                                                 Scan = scanCode,
                                                 Flags =
                                                     (UInt32)(WinAPI.KeyboardFlag.KeyUp | WinAPI.KeyboardFlag.Unicode),
                                                 Time = 0,
                                                 ExtraInfo = IntPtr.Zero
                                             }
                                 }
            };

            // Handle extended keys:
            // If the scan code is preceded by a prefix byte that has the value 0xE0 (224),
            // we need to include the KEYEVENTF_EXTENDEDKEY flag in the Flags property. 
            if ( (scanCode & 0xFF00) == 0xE000 ) {
                down.Data.Keyboard.Flags |= (UInt32)WinAPI.KeyboardFlag.ExtendedKey;
                up.Data.Keyboard.Flags |= (UInt32)WinAPI.KeyboardFlag.ExtendedKey;
            }

            INPUTS.Add( down );
            INPUTS.Add( up );
            return this;
        }

        public Macro Add(string text) {
            foreach ( var character in text )
                Add( character );
            return this;
        }

        public Macro Run() {
            List<WinAPI.INPUT> k = new List<WinAPI.INPUT>();
            foreach ( var x in INPUTS ) {
                if( x.Type != (UInt32)WinAPI.InputType.Sleep ) {
                    k.Add( x );
                } else {
                    var result = WinAPI.SendInput( (UInt32)k.Count, k.ToArray(), System.Runtime.InteropServices.Marshal.SizeOf( typeof( WinAPI.INPUT ) ) );
                    Thread.Sleep( (int)x.Data.Keyboard.Flags );
                    k.Clear();
                }
            }
            if ( k.Count > 0 ) {
                WinAPI.SendInput( (UInt32)k.Count, k.ToArray(), System.Runtime.InteropServices.Marshal.SizeOf( typeof( WinAPI.INPUT ) ) );
            }
            return this;
        }
    }
}
