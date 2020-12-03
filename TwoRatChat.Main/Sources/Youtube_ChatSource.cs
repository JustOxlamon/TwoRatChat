// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;
using System.Xml.Linq;
using TwoRatChat.Controls;

namespace TwoRatChat.Main.Sources {
    public class Youtube_ChatSource : YoutubeGaming_ChatSource {
        public Youtube_ChatSource(Dispatcher dispatcher)
            : base(dispatcher) {

        }
    }
}