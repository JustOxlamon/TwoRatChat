using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dotIRC
{
    internal class MessageProcessorAttribute : Attribute
    {
        public MessageProcessorAttribute(string commandName)
        {
            this.CommandName = commandName;
        }

        public string CommandName
        {
            get;
            private set;
        }
    }
}
