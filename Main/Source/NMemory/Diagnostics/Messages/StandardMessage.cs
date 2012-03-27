using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Diagnostics.Messages
{
    public class StandardMessage : Message
    {
        public StandardMessage(string text)
        {
            this.Text = text;
        }

        public StandardMessage(string text, params object[] args)
        {
            this.Text = string.Format(text, args);
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("*** {0} ***", this.Text);
        }
    }
}
