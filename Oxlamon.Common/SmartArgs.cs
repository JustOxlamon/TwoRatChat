using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxlamon.Common {
    public class SmartArgs: Dictionary<string, string> {
        
        public SmartArgs() {
            string[] args = Environment.GetCommandLineArgs();
            foreach (var x in args) {
                string[] d = x.Split( '=' );

                if (d.Length == 1)
                    this[d[0]] = string.Empty;
                else
                    if (d.Length == 2)
                        this[d[0]] = d[1];
            }
        }


    }
}
