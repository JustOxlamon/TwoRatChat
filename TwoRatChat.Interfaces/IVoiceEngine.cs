using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoRatChat.Interfaces {
    public interface IVoiceEngine {
        void BeginInitialize(string locale);

        void Talk(string voice, string text);

        void EndInitialize();

        void Register(object voiceActuator, CultureInfo _locale, string _start, string[] _phrases);

        void Unregister(object voiceActuator);

        event Action<object> OnRecognize;

        List<string> Voices { get; }
    }
}
