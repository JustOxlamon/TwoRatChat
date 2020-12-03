using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Commands {
    static class WinAPI {
        internal enum InputType : uint {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2,
            Sleep = 3 // inernal
        }

        [Flags]
        internal enum KeyboardFlag : uint {
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            ScanCode = 0x0008,
        }

        internal enum MapVK : uint {
            VK_TO_VSC = 0x00,
            MAPVK_VSC_TO_VK = 0x01,
            MAPVK_VK_TO_CHAR = 0x02,
            MAPVK_VSC_TO_VK_EX = 0x03,
            MAPVK_VK_TO_VSC_EX = 0x04
        }

#pragma warning disable 649
    internal struct HARDWAREINPUT {
            public UInt32 Msg;
            public UInt16 ParamL;
            public UInt16 ParamH;
        }

        internal struct MOUSEINPUT {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        internal struct KEYBDINPUT {
            public UInt16 KeyCode;
            public UInt16 Scan;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout( LayoutKind.Explicit )]
        internal struct MOUSEKEYBDHARDWAREINPUT {
            [FieldOffset( 0 )]
            public MOUSEINPUT Mouse;
            [FieldOffset( 0 )]
            public KEYBDINPUT Keyboard;
            [FieldOffset( 0 )]
            public HARDWAREINPUT Hardware;
        }

        internal struct INPUT {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }
#pragma warning restore 649

        [DllImport( "user32.dll", SetLastError = true )]
        public static extern Int16 GetAsyncKeyState(UInt16 virtualKeyCode);

        [DllImport( "user32.dll", SetLastError = true )]
        public static extern Int16 GetKeyState(UInt16 virtualKeyCode);

        [DllImport( "user32.dll", SetLastError = true )]
        public static extern UInt32 SendInput(UInt32 numberOfInputs, INPUT[] inputs, Int32 sizeOfInputStructure);

        [DllImport( "user32.dll" )]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
        public static extern IntPtr GetForegroundWindow();

        [DllImport( "user32.dll" )]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport( "user32.dll" )]
        public static extern uint MapVirtualKey(VirtualKeyCode uCode, MapVK uMapType);


        public static string GetForegroundWindowText() {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder( nChars );
            var hwnd = WinAPI.GetForegroundWindow();

            if ( WinAPI.GetWindowText( hwnd, Buff, nChars ) > 0 )
                return Buff.ToString();

            return string.Empty;
        }
    }

    public enum VirtualKeyCode: UInt32 {
        //
        // Сводка:
        //     Битовая маска для извлечения модификаторы от значения ключа.
      //  Modifiers = -65536,
        //
        // Сводка:
        //     Нет отжатого ключа.
        None = 0,
        //
        // Сводка:
        //     Левая кнопка мыши.
        LButton = 1,
        //
        // Сводка:
        //     Правая кнопка мыши.
        RButton = 2,
        //
        // Сводка:
        //     Кнопка digit устройства.
        Cancel = 3,
        //
        // Сводка:
        //     Средняя кнопка мыши (мыши 3 кнопок).
        MButton = 4,
        //
        // Сводка:
        //     Первая кнопка мыши x (5 кнопки мыши.
        XButton1 = 5,
        //
        // Сводка:
        //     Вторая кнопка мыши x (5 кнопки мыши.
        XButton2 = 6,
        //
        // Сводка:
        //     Клавиша BACKSPACE.
        Back = 8,
        //
        // Сводка:
        //     Клавиша табуляции.
        Tab = 9,
        //
        // Сводка:
        //     Ключ ПЕРЕВОДА СТРОКИ.
        LineFeed = 10,
        //
        // Сводка:
        //     Незащищенный ключ.
        Clear = 12,
        //
        // Сводка:
        //     ВОЗВРАЩАЕМЫЙ ключ.
        Return = 13,
        //
        // Сводка:
        //     Клавиша ВВОД.
        Enter = 13,
        //
        // Сводка:
        //     Клавиша SHIFT.
        ShiftKey = 16,
        //
        // Сводка:
        //     Ключ CTRL.
        ControlKey = 17,
        //
        // Сводка:
        //     Ключ ALT.
        Menu = 18,
        //
        // Сводка:
        //     Ключ ПРИОСТАНОВКИ.
        Pause = 19,
        //
        // Сводка:
        //     Клавиша CAPS LOCK.
        Capital = 20,
        //
        // Сводка:
        //     Клавиша CAPS LOCK.
        CapsLock = 20,
        //
        // Сводка:
        //     Ключ режим IME японской азбуки.
        KanaMode = 21,
        //
        // Сводка:
        //     Ключ режим IME Hanguel.(поддерживается для обеспечения совместимости; используйте
        //     HangulMode)
        HanguelMode = 21,
        //
        // Сводка:
        //     Ключ режим IME хангыль.
        HangulMode = 21,
        //
        // Сводка:
        //     Ключ режим IME Junja.
        JunjaMode = 23,
        //
        // Сводка:
        //     Режим IME окончательный ключ.
        FinalMode = 24,
        //
        // Сводка:
        //     Ключ режим IME Ханджа.
        HanjaMode = 25,
        //
        // Сводка:
        //     Ключ режима Кандзи IME.
        KanjiMode = 25,
        //
        // Сводка:
        //     Ключ ESC.
        Escape = 27,
        //
        // Сводка:
        //     Ключ convert IME.
        IMEConvert = 28,
        //
        // Сводка:
        //     Ключ nonconvert IME.
        IMENonconvert = 29,
        //
        // Сводка:
        //     IME принимает ключ, заменяет System.Windows.Forms.Keys.IMEAceept.
        IMEAccept = 30,
        //
        // Сводка:
        //     IME принимает ключ.Является устаревшим, используйте System.Windows.Forms.Keys.IMEAccept
        //     вместо этого.
        IMEAceept = 30,
        //
        // Сводка:
        //     Ключ изменения режим IME.
        IMEModeChange = 31,
        //
        // Сводка:
        //     Ключ пробелов.
        Space = 32,
        //
        // Сводка:
        //     СТРАНИЦА ВВЕРХ пользуется ключом.
        Prior = 33,
        //
        // Сводка:
        //     СТРАНИЦА ВВЕРХ пользуется ключом.
        PageUp = 33,
        //
        // Сводка:
        //     Стрелка вниз PAGE.
        Next = 34,
        //
        // Сводка:
        //     Стрелка вниз PAGE.
        PageDown = 34,
        //
        // Сводка:
        //     Ключ ЭЛЕМЕНТА.
        End = 35,
        //
        // Сводка:
        //     Ключ КОРНЕВОЙ ПАПКИ.
        Home = 36,
        //
        // Сводка:
        //     Ключ ВЛЕВО.
        Left = 37,
        //
        // Сводка:
        //     Нажатии клавиши со стрелками.
        Up = 38,
        //
        // Сводка:
        //     Ключ СТРЕЛКА ВПРАВО ".
        Right = 39,
        //
        // Сводка:
        //     Клавиши со стрелками.
        Down = 40,
        //
        // Сводка:
        //     ВЫБОР ключа.
        Select = 41,
        //
        // Сводка:
        //     Ключ ПЕЧАТИ.
        Print = 42,
        //
        // Сводка:
        //     Ключ ВЫПОЛНЕНИЯ.
        Execute = 43,
        //
        // Сводка:
        //     Клавиша PRINT SCREEN.
        Snapshot = 44,
        //
        // Сводка:
        //     Клавиша PRINT SCREEN.
        PrintScreen = 44,
        //
        // Сводка:
        //     Ключ INS.
        Insert = 45,
        //
        // Сводка:
        //     DEL ключ.
        Delete = 46,
        //
        // Сводка:
        //     Клавиша справки.
        Help = 47,
        //
        // Сводка:
        //     0 Ключей.
        D0 = 48,
        //
        // Сводка:
        //     1 Ключ.
        D1 = 49,
        //
        // Сводка:
        //     Ключ 2.
        D2 = 50,
        //
        // Сводка:
        //     Ключ 3.
        D3 = 51,
        //
        // Сводка:
        //     Ключ 4.
        D4 = 52,
        //
        // Сводка:
        //     Ключ 5.
        D5 = 53,
        //
        // Сводка:
        //     Ключ 6.
        D6 = 54,
        //
        // Сводка:
        //     Ключ 7.
        D7 = 55,
        //
        // Сводка:
        //     Ключ 8.
        D8 = 56,
        //
        // Сводка:
        //     Ключ 9.
        D9 = 57,
        //
        // Сводка:
        //     Ключ a.
        A = 65,
        //
        // Сводка:
        //     Ключ б.
        B = 66,
        //
        // Сводка:
        //     Ключ c#.
        C = 67,
        //
        // Сводка:
        //     Ключ D.
        D = 68,
        //
        // Сводка:
        //     Ключ для всех.
        E = 69,
        //
        // Сводка:
        //     Ключ f.
        F = 70,
        //
        // Сводка:
        //     Общий ключ.
        G = 71,
        //
        // Сводка:
        //     Ключ з.
        H = 72,
        //
        // Сводка:
        //     I пользуюсь ключом.
        I = 73,
        //
        // Сводка:
        //     Ключ к.
        J = 74,
        //
        // Сводка:
        //     Ключ л.
        K = 75,
        //
        // Сводка:
        //     l ключ.
        L = 76,
        //
        // Сводка:
        //     Ключ M.
        M = 77,
        //
        // Сводка:
        //     Ключ - n.
        N = 78,
        //
        // Сводка:
        //     Ключ o.
        O = 79,
        //
        // Сводка:
        //     Ключ р.
        P = 80,
        //
        // Сводка:
        //     Ключ q.
        Q = 81,
        //
        // Сводка:
        //     Ключ r.
        R = 82,
        //
        // Сводка:
        //     Ключ s.
        S = 83,
        //
        // Сводка:
        //     Ключ типа t.
        T = 84,
        //
        // Сводка:
        //     Ключ U.
        U = 85,
        //
        // Сводка:
        //     Ключ - v.
        V = 86,
        //
        // Сводка:
        //     Ключ w.
        W = 87,
        //
        // Сводка:
        //     Ключ x.
        X = 88,
        //
        // Сводка:
        //     Ключ y.
        Y = 89,
        //
        // Сводка:
        //     Ключ Z.
        Z = 90,
        //
        // Сводка:
        //     Левый ключ эмблемы windows (клавиатура microsoft естественная).
        LWin = 91,
        //
        // Сводка:
        //     Правый ключ эмблемы windows (клавиатура microsoft естественная).
        RWin = 92,
        //
        // Сводка:
        //     Ключ приложения (клавиатура microsoft естественная).
        Apps = 93,
        //
        // Сводка:
        //     Ключ сна компьютера.
        Sleep = 95,
        //
        // Сводка:
        //     0 Ключей на цифровой клавиатуре.
        NumPad0 = 96,
        //
        // Сводка:
        //     1 Ключ на цифровой клавиатуре.
        NumPad1 = 97,
        //
        // Сводка:
        //     Ключ 2 на цифровой клавиатуре.
        NumPad2 = 98,
        //
        // Сводка:
        //     Ключ 3 на цифровой клавиатуре.
        NumPad3 = 99,
        //
        // Сводка:
        //     Ключ 4 на цифровой клавиатуре.
        NumPad4 = 100,
        //
        // Сводка:
        //     Ключ 5 на цифровой клавиатуре.
        NumPad5 = 101,
        //
        // Сводка:
        //     Ключ 6 на цифровой клавиатуре.
        NumPad6 = 102,
        //
        // Сводка:
        //     Ключ 7 на цифровой клавиатуре.
        NumPad7 = 103,
        //
        // Сводка:
        //     Ключ 8 на цифровой клавиатуре.
        NumPad8 = 104,
        //
        // Сводка:
        //     Ключ 9 на цифровой клавиатуре.
        NumPad9 = 105,
        //
        // Сводка:
        //     Ключ умножения.
        Multiply = 106,
        //
        // Сводка:
        //     Ключ добавить.
        Add = 107,
        //
        // Сводка:
        //     Ключ разделителя.
        Separator = 108,
        //
        // Сводка:
        //     Ключ вычитания.
        Subtract = 109,
        //
        // Сводка:
        //     Десятичный ключ.
        Decimal = 110,
        //
        // Сводка:
        //     Ключ деление.
        Divide = 111,
        //
        // Сводка:
        //     Ключ F1.
        F1 = 112,
        //
        // Сводка:
        //     Ключ F2.
        F2 = 113,
        //
        // Сводка:
        //     Ключ F3.
        F3 = 114,
        //
        // Сводка:
        //     Ключ F4.
        F4 = 115,
        //
        // Сводка:
        //     Ключ F5.
        F5 = 116,
        //
        // Сводка:
        //     Ключ F6.
        F6 = 117,
        //
        // Сводка:
        //     Ключ F7.
        F7 = 118,
        //
        // Сводка:
        //     Ключ F8.
        F8 = 119,
        //
        // Сводка:
        //     Ключ F9.
        F9 = 120,
        //
        // Сводка:
        //     Ключ F10.
        F10 = 121,
        //
        // Сводка:
        //     Ключ F11.
        F11 = 122,
        //
        // Сводка:
        //     Ключ F12.
        F12 = 123,
        //
        // Сводка:
        //     Ключ F13.
        F13 = 124,
        //
        // Сводка:
        //     F14 ключ.
        F14 = 125,
        //
        // Сводка:
        //     F15 ключ.
        F15 = 126,
        //
        // Сводка:
        //     F16 ключ.
        F16 = 127,
        //
        // Сводка:
        //     F17 ключ.
        F17 = 128,
        //
        // Сводка:
        //     F18 ключ.
        F18 = 129,
        //
        // Сводка:
        //     F19 ключ.
        F19 = 130,
        //
        // Сводка:
        //     F20 ключ.
        F20 = 131,
        //
        // Сводка:
        //     F21 ключ.
        F21 = 132,
        //
        // Сводка:
        //     F22 ключ.
        F22 = 133,
        //
        // Сводка:
        //     F23 ключ.
        F23 = 134,
        //
        // Сводка:
        //     F24 ключ.
        F24 = 135,
        //
        // Сводка:
        //     Клавиша NUM LOCK.
        NumLock = 144,
        //
        // Сводка:
        //     Клавиша SCROLL LOCK.
        Scroll = 145,
        //
        // Сводка:
        //     Левая клавиша SHIFT.
        LShiftKey = 160,
        //
        // Сводка:
        //     Правая клавиша SHIFT.
        RShiftKey = 161,
        //
        // Сводка:
        //     Ключ CTRL слева.
        LControlKey = 162,
        //
        // Сводка:
        //     Правый ключ CTRL.
        RControlKey = 163,
        //
        // Сводка:
        //     Ключ ALT слева.
        LMenu = 164,
        //
        // Сводка:
        //     Правый ключ ALT.
        RMenu = 165,
        //
        // Сводка:
        //     Ключ обратно браузера (Windows версии 2000 или более поздней версии).
        BrowserBack = 166,
        //
        // Сводка:
        //     Ключ браузера передний (Windows версии 2000 или более поздней версии).
        BrowserForward = 167,
        //
        // Сводка:
        //     Браузер обновляет ключ (Windows версии 2000 или более поздней версии).
        BrowserRefresh = 168,
        //
        // Сводка:
        //     Ключ остановки браузера (Windows версии 2000 или более поздней версии).
        BrowserStop = 169,
        //
        // Сводка:
        //     Ключ поиска браузера (Windows версии 2000 или более поздней версии).
        BrowserSearch = 170,
        //
        // Сводка:
        //     Ключ " избранное " браузера (Windows версии 2000 или более поздней версии).
        BrowserFavorites = 171,
        //
        // Сводка:
        //     Ключ главного окна браузера (Windows версии 2000 или более поздней версии).
        BrowserHome = 172,
        //
        // Сводка:
        //     Ключ отсоединение томов звука (Windows версии 2000 или более поздней версии).
        VolumeMute = 173,
        //
        // Сводка:
        //     Тома ключ вниз (Windows версии 2000 или более поздней версии).
        VolumeDown = 174,
        //
        // Сводка:
        //     Тома ключ вверх (Windows версии 2000 или более поздней версии).
        VolumeUp = 175,
        //
        // Сводка:
        //     Носителя ключ отслеживания далее (Windows версии 2000 или более поздней версии).
        MediaNextTrack = 176,
        //
        // Сводка:
        //     Ключ отслеживания носителя предыдущий (Windows версии 2000 или более поздней
        //     версии).
        MediaPreviousTrack = 177,
        //
        // Сводка:
        //     Ключ остановки носителя (Windows версии 2000 или более поздней версии).
        MediaStop = 178,
        //
        // Сводка:
        //     Ключ паузы игры носителя (Windows версии 2000 или более поздней версии).
        MediaPlayPause = 179,
        //
        // Сводка:
        //     Ключ почты запуска (Windows версии 2000 или более поздней версии).
        LaunchMail = 180,
        //
        // Сводка:
        //     Выбор ключа носителя (Windows версии 2000 или более поздней версии).
        SelectMedia = 181,
        //
        // Сводка:
        //     Ключ начального одного приложения Windows 2000 (или более поздняя версия).
        LaunchApplication1 = 182,
        //
        // Сводка:
        //     Ключ приложения начала 2 (Windows версии 2000 или более поздней версии).
        LaunchApplication2 = 183,
        //
        // Сводка:
        //     Ключ точки с запятой OEM на стандартной клавиатуре США (Windows версии 2000 или
        //     более поздней версии).
        OemSemicolon = 186,
        //
        // Сводка:
        //     Ключ OEM 1.
        Oem1 = 186,
        //
        // Сводка:
        //     OEM и ключ для любой клавиатуре страны или региона (Windows версии 2000 или более
        //     поздней версии).
        Oemplus = 187,
        //
        // Сводка:
        //     Ключ запятой OEM в любой клавиатуре страны или региона (Windows версии 2000 или
        //     более поздней версии).
        Oemcomma = 188,
        //
        // Сводка:
        //     OEM минус ключ для любой клавиатуре страны или региона (Windows версии 2000 или
        //     более поздней версии).
        OemMinus = 189,
        //
        // Сводка:
        //     Ключ периода OEM в любой клавиатуре страны или региона (Windows версии 2000 или
        //     более поздней версии).
        OemPeriod = 190,
        //
        // Сводка:
        //     Ключ вопроса OEM на стандартной клавиатуре США (Windows версии 2000 или более
        //     поздней версии).
        OemQuestion = 191,
        //
        // Сводка:
        //     Ключ OEM 2.
        Oem2 = 191,
        //
        // Сводка:
        //     Ключ тильда OEM на стандартной клавиатуре США (Windows версии 2000 или более
        //     поздней версии).
        Oemtilde = 192,
        //
        // Сводка:
        //     Ключ OEM 3.
        Oem3 = 192,
        //
        // Сводка:
        //     Ключ брекета открытого OEM на стандартной клавиатуре США (Windows версии 2000
        //     или более поздней версии).
        OemOpenBrackets = 219,
        //
        // Сводка:
        //     Ключ OEM 4.
        Oem4 = 219,
        //
        // Сводка:
        //     Ключ канала OEM на стандартной клавиатуре США (Windows версии 2000 или более
        //     поздней версии).
        OemPipe = 220,
        //
        // Сводка:
        //     Ключ OEM 5.
        Oem5 = 220,
        //
        // Сводка:
        //     Ключ заключительной квадратной скобки OEM на стандартной клавиатуре США (Windows
        //     версии 2000 или более поздней версии).
        OemCloseBrackets = 221,
        //
        // Сводка:
        //     Ключ OEM 6.
        Oem6 = 221,
        //
        // Сводка:
        //     OEM указан ключ/двойной кавычки в стандартной клавиатуре США (Windows версии
        //     2000 или более поздней версии).
        OemQuotes = 222,
        //
        // Сводка:
        //     Ключ OEM 7.
        Oem7 = 222,
        //
        // Сводка:
        //     Ключ OEM 8.
        Oem8 = 223,
        //
        // Сводка:
        //     Ключ стенного угольника или обратной косой черты OEM на клавиатуре 102 RT ключа
        //     (Windows версии 2000 или более поздней версии).
        OemBackslash = 226,
        //
        // Сводка:
        //     Ключ OEM 102.
        Oem102 = 226,
        //
        // Сводка:
        //     Ключ ПАРЫ ПРОЦЕССА.
        ProcessKey = 229,
        //
        // Сводка:
        //     Используется для передачи символов юникода, если они были нажатиями клавиш.Значение
        //     ключа пакета, нижней машинное слово пакетом обновления 32 (sp2) значения виртуальный-ключа,
        //     используемого для методов ввода non-клавиатуры.
        Packet = 231,
        //
        // Сводка:
        //     Ключ ATTN.
        Attn = 246,
        //
        // Сводка:
        //     Ключ CRSEL.
        Crsel = 247,
        //
        // Сводка:
        //     Ключ EXSEL.
        Exsel = 248,
        //
        // Сводка:
        //     Ключ EOF ERASE.
        EraseEof = 249,
        //
        // Сводка:
        //     Ключ ВОСПРОИЗВЕСТИ.
        Play = 250,
        //
        // Сводка:
        //     Ключ УВЕЛИЧЕНИЯ.
        Zoom = 251,
        //
        // Сводка:
        //     Константа зарезервированная для использования в будущем.
        NoName = 252,
        //
        // Сводка:
        //     Ключ PA1.
        Pa1 = 253,
        //
        // Сводка:
        //     Незащищенный ключ.
        OemClear = 254,
        //
        // Сводка:
        //     Битовая маска для извлечения ключевой код из значений ключа.
        KeyCode = 65535,
        //
        // Сводка:
        //     Клавиша-модификатор МИГРАЦИИ.
        Shift = 65536,
        //
        // Сводка:
        //     Клавиша-модификатор CTRL.
        Control = 131072,
        //
        // Сводка:
        //     Клавиша-модификатор ALT.
        Alt = 262144
    }

    enum VirtualScanCode {
        Escape = 0x01
, D1 = 0x02
, D2 = 0x03
, D3 = 0x04
, D4 = 0x05
, D5 = 0x06
, D6 = 0x07
, D7 = 0x08
, D8 = 0x09
, D9 = 0x0A
, D0 = 0x0B
, MINUS = 0x0C    /* - on main keyboard */
, EQUALS = 0x0D
, BACK = 0x0E    /* backspace */
, TAB = 0x0F
, Q = 0x10
, W = 0x11
, E = 0x12
, R = 0x13
, T = 0x14
, Y = 0x15
, U = 0x16
, I = 0x17
, O = 0x18
, P = 0x19
, LBRACKET = 0x1A
, RBRACKET = 0x1B
, RETURN = 0x1C    /* Enter on main keyboard */
, LCONTROL = 0x1D
, A = 0x1E
, S = 0x1F
, D = 0x20
, F = 0x21
, G = 0x22
, H = 0x23
, J = 0x24
, K = 0x25
, L = 0x26
, SEMICOLON = 0x27
, APOSTROPHE = 0x28
, GRAVE = 0x29    /* accent grave */
, LSHIFT = 0x2A
, BACKSLASH = 0x2B
, Z = 0x2C
, X = 0x2D
, C = 0x2E
, V = 0x2F
, B = 0x30
, N = 0x31
, M = 0x32
, COMMA = 0x33
, PERIOD = 0x34    /* . on main keyboard */
, SLASH = 0x35    /* / on main keyboard */
, RSHIFT = 0x36
, MULTIPLY = 0x37    /* * on numeric keypad */
, LMENU = 0x38    /* left Alt */
, SPACE = 0x39
, CAPITAL = 0x3A
, F1 = 0x3B
, F2 = 0x3C
, F3 = 0x3D
, F4 = 0x3E
, F5 = 0x3F
, F6 = 0x40
, F7 = 0x41
, F8 = 0x42
, F9 = 0x43
, F10 = 0x44
, NUMLOCK = 0x45
, SCROLL = 0x46    /* Scroll Lock */
, NUMPAD7 = 0x47
, NUMPAD8 = 0x48
, NUMPAD9 = 0x49
, SUBTRACT = 0x4A    /* - on numeric keypad */
, NUMPAD4 = 0x4B
, NUMPAD5 = 0x4C
, NUMPAD6 = 0x4D
, ADD = 0x4E    /* + on numeric keypad */
, NUMPAD1 = 0x4F
, NUMPAD2 = 0x50
, NUMPAD3 = 0x51
, NUMPAD0 = 0x52
, DECIMAL = 0x53    /* . on numeric keypad */
, OEM_102 = 0x56    /* <> or \| on RT 102-key keyboard (Non-U.S.) */
, F11 = 0x57
, F12 = 0x58
, F13 = 0x64    /*                     (NEC PC98) */
, F14 = 0x65    /*                     (NEC PC98) */
, F15 = 0x66    /*                     (NEC PC98) */
, KANA = 0x70    /* (Japanese keyboard)            */
, ABNT_C1 = 0x73    /* /? on Brazilian keyboard */
, CONVERT = 0x79    /* (Japanese keyboard)            */
, NOCONVERT = 0x7B    /* (Japanese keyboard)            */
, YEN = 0x7D    /* (Japanese keyboard)            */
, ABNT_C2 = 0x7E    /* Numpad . on Brazilian keyboard */
, NUMPADEQUALS = 0x8D    /* = on numeric keypad (NEC PC98) */
, PREVTRACK = 0x90    /* Previous Track (CIRCUMFLEX on Japanese keyboard) */
, AT = 0x91    /*                     (NEC PC98) */
, COLON = 0x92    /*                     (NEC PC98) */
, UNDERLINE = 0x93    /*                     (NEC PC98) */
, KANJI = 0x94    /* (Japanese keyboard)            */
, STOP = 0x95    /*                     (NEC PC98) */
, AX = 0x96    /*                     (Japan AX) */
, UNLABELED = 0x97    /*                        (J3100) */
, NEXTTRACK = 0x99    /* Next Track */
, NUMPADENTER = 0x9C    /* Enter on numeric keypad */
, RCONTROL = 0x9D
, MUTE = 0xA0    /* Mute */
, CALCULATOR = 0xA1    /* Calculator */
, PLAYPAUSE = 0xA2    /* Play / Pause */
, MEDIASTOP = 0xA4    /* Media Stop */
, VOLUMEDOWN = 0xAE    /* Volume - */
, VOLUMEUP = 0xB0    /* Volume + */
, WEBHOME = 0xB2    /* Web home */
, NUMPADCOMMA = 0xB3    /* , on numeric keypad (NEC PC98) */
, DIVIDE = 0xB5    /* / on numeric keypad */
, SYSRQ = 0xB7
, RMENU = 0xB8    /* right Alt */
, PAUSE = 0xC5    /* Pause */
, HOME = 0xC7    /* Home on arrow keypad */
, UP = 0xC8    /* UpArrow on arrow keypad */
, PRIOR = 0xC9    /* PgUp on arrow keypad */
, LEFT = 0xCB    /* LeftArrow on arrow keypad */
, RIGHT = 0xCD    /* RightArrow on arrow keypad */
, END = 0xCF    /* End on arrow keypad */
, DOWN = 0xD0    /* DownArrow on arrow keypad */
, NEXT = 0xD1    /* PgDn on arrow keypad */
, INSERT = 0xD2    /* Insert on arrow keypad */
, DELETE = 0xD3    /* Delete on arrow keypad */
, LWIN = 0xDB    /* Left Windows key */
, RWIN = 0xDC    /* Right Windows key */
, APPS = 0xDD    /* AppMenu key */
, POWER = 0xDE    /* System Power */
, SLEEP = 0xDF    /* System Sleep */
, WAKE = 0xE3    /* System Wake */
, WEBSEARCH = 0xE5    /* Web Search */
, WEBFAVORITES = 0xE6    /* Web Favorites */
, WEBREFRESH = 0xE7    /* Web Refresh */
, WEBSTOP = 0xE8    /* Web Stop */
, WEBFORWARD = 0xE9    /* Web Forward */
, WEBBACK = 0xEA    /* Web Back */
, MYCOMPUTER = 0xEB    /* My Computer */
, MAIL = 0xEC    /* Mail */
, MEDIASELECT = 0xED    /* Media Select */
    }
}
