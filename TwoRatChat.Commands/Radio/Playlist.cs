using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Oxlamon.Radio {
    public class Channel: List<string> {
        Random rnd = new Random();

        public string Name { get; set; }

        public Channel() {
        }

        public void Load( XElement x ) {
            Clear();
            this.Name = x.Attribute("name").Value;
            foreach (var i in x.Elements())
                Add(i.Attribute("value").Value);
        }

        public XElement ToXElement() {
            XElement c = new XElement("channel", new XAttribute("name", Name));

            foreach (var i in this)
                c.Add(new XElement("url", new XAttribute("value", i)));
            return c;
        }

        public string GetRandom() {
            return this[rnd.Next(this.Count)];
        }
    }

    public class Playlist : List<Channel> {
        Random rnd = new Random();

        public Channel this[string name] {
            get {
                foreach (var c in this)
                    if (c.Name == name)
                        return c;
                return null;
            }
        }

        public string GetRandom() {
            return this[rnd.Next(this.Count)].GetRandom();
        }

        public void Load( XElement x ) {
            foreach (var i in x.Elements()) {
                Channel c = new Channel();
                c.Load(i);
                Add(c);
            }
        }

        public XElement ToXElement() {
            XElement x = new XElement("playlist");
            foreach (var i in this)
                x.Add(i.ToXElement());
            return x;
        }
    }
}
