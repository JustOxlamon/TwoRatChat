using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace TwoRatChat.Main.Helpers {
    class TransparentMouse {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);
        private int _NormalWindowStyle;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc( int nCode, IntPtr wParam, IntPtr lParam );
        private static Window window;
        private bool _AllowTransparency = false;
        private static TransparentMouse This;

        public TransparentMouse( Window window ) {
            TransparentMouse.window = window;
           
            This = this;
            IntPtr hwnd = new WindowInteropHelper( window ).Handle;
            _NormalWindowStyle = GetWindowLong( hwnd, GWL_EXSTYLE );
        }

        public bool AllowTransparency {
            get {
                return _AllowTransparency;
            }
            set {
                if (_hookID != IntPtr.Zero) {
                    UnhookWindowsHookEx( _hookID );
                    _hookID = IntPtr.Zero;
                    setUnTransparent();
                }

                _AllowTransparency = value;


                if (_AllowTransparency) {
                    _hookID = SetHook();
                    setTransparent();
                }
            }
        }

        [DllImport( "user32.dll" )]
        internal static extern int SetWindowLong( IntPtr hwnd, int index, int newStyle );
        [DllImport( "user32.dll" )]
        internal static extern int GetWindowLong( IntPtr hwnd, int index );
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        private static extern IntPtr SetWindowsHookEx( int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId );
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool UnhookWindowsHookEx( IntPtr hhk );
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        private static extern IntPtr CallNextHookEx( IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam );
        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        private static extern IntPtr GetModuleHandle( string lpModuleName );

        private IntPtr SetHook() {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx( WH_KEYBOARD_LL, _proc,
                    GetModuleHandle( curModule.ModuleName ), 0 );
            }
        }

        private static IntPtr HookCallback( int nCode, IntPtr wParam, IntPtr lParam ) {

            if (This._AllowTransparency) {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                    int vkCode = Marshal.ReadInt32( lParam );
                    if (vkCode == 162)
                        This.setUnTransparent();
                } else
                    if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
                        int vkCode = Marshal.ReadInt32( lParam );
                        if (vkCode == 162)
                            This.setTransparent();
                    }
            }

            return CallNextHookEx( _hookID, nCode, wParam, lParam );
        }

        void setTransparent() {
            IntPtr hwnd = new WindowInteropHelper( window ).Handle;
            SetWindowLong( hwnd, GWL_EXSTYLE, _NormalWindowStyle | WS_EX_TRANSPARENT );
        }

        void setUnTransparent() {
            IntPtr hwnd = new WindowInteropHelper( window ).Handle;
            SetWindowLong( hwnd, GWL_EXSTYLE, _NormalWindowStyle );
        }
    }
}
