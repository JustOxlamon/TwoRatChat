using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TwoRatChat.Main.Sources {
    public class RealTV_Source : TwoRatChat.Model.ChatSource {
        public RealTV_Source(Dispatcher dispatcher)
            : base(dispatcher) {
        }

        public override string Id { get { return "realtv"; } }

        public override void Create(string streamerUri, string id) {
        }

        public override void Destroy() {
        }

        public override void UpdateViewerCount() {
        }
    }
}
