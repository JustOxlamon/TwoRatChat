using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core;

namespace TwoRatChat.Main.Controls {
    public class HtmlViewControl: Awesomium.Windows.Controls.WebControl {
        
    }
    public class ddd : Awesomium.Core.INavigationInterceptor {
        public string[] Blacklist {
            get {
                throw new NotImplementedException();
            }
        }

        public NavigationRule ImplicitRule {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public string[] Whitelist {
            get {
                throw new NotImplementedException();
            }
        }

        public event BeginLoadingFrameEventHandler BeginLoadingFrame;
        public event BeginNavigationEventHandler BeginNavigation;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public CollectionChangeAction AddRule(NavigationFilterRule filterRule) {
            throw new NotImplementedException();
        }

        public CollectionChangeAction AddRule(string pattern, NavigationRule rule) {
            throw new NotImplementedException();
        }

        public int AddRules(params NavigationFilterRule[] rules) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(string pattern) {
            throw new NotImplementedException();
        }

        public IEnumerator<NavigationFilterRule> GetEnumerator() {
            throw new NotImplementedException();
        }

        public NavigationRule GetRule(string url) {
            throw new NotImplementedException();
        }

        public int RemoveRules(string pattern) {
            throw new NotImplementedException();
        }

        public int RemoveRules(string pattern, NavigationRule rule) {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
