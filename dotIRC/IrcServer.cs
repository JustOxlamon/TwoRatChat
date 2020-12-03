﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotIRC
{
    /// <summary>
    /// Represents an IRC server from the view of a particular client.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class IrcServer : IIrcMessageSource
    {
        private string hostName;

        internal IrcServer(string hostName)
        {
            this.hostName = hostName;
        }

        /// <summary>
        /// Gets the host name of the server.
        /// </summary>
        /// <value>The host name of the server.</value>
        public string HostName
        {
            get { return this.hostName; }
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>A string that represents this instance.</returns>
        public override string ToString()
        {
            return this.hostName;
        }

        #region IIrcMessageSource Members

        string IIrcMessageSource.Name
        {
            get { return this.HostName; }
        }

        #endregion
    }
}
