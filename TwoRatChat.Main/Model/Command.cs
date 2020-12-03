// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwoRatChat.Model {
    public class Command {
        [JsonProperty("gid")]
        public readonly Guid GID = Guid.NewGuid();
        [JsonProperty( "type" )]
        public readonly string Type;
        [JsonProperty( "prms" )]
        public readonly string[] Params;

        public Command( string type, params string[] prms) {
            this.Type = type;
            this.Params = prms;
        }

        public string ToJson( bool allowRatSource ) {
            return JsonConvert.SerializeObject( this );
        }
    }
}
