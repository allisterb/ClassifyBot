using System;
using System.Collections.Generic;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public abstract class Classifier
    {
        public ILogger L { get; protected set; }
    }
}
