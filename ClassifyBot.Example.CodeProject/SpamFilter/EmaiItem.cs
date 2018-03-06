using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot.Pipeline.CodeProject.SpamFilter
{
    public class EmailItem : Record<string>
    {
        public EmailItem(int lineNo, string label, string text) : base(lineNo, label, text) {}
    }
}
