using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwoRatChat.Update {
    class FileItem {
        public readonly long crc;
        public readonly long zipCrc;
        public readonly string fileName;

        public FileItem(XElement x) {
            this.fileName = x.Attribute("path").Value;
            this.crc = long.Parse(x.Attribute("crc").Value);
            this.zipCrc = long.Parse( x.Attribute( "zcrc" ).Value );
        }

        public FileItem(string fileName, long crc, long zipCrc) {
            this.fileName = fileName;
            this.crc = crc;
            this.zipCrc = zipCrc;
        }

        public XElement ToXElement() {
            return new XElement("file",
                new XAttribute("path", this.fileName),
                new XAttribute("crc", this.crc),
                new XAttribute("zcrc", this.zipCrc));
        }

        public override string ToString() {
            return string.Format( "{0} ({1:X}/{2:X})", this.fileName, this.crc, this.zipCrc );
        }
    }
}
