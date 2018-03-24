using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public interface IExtern<T>
    {
        Extern<T> E { get; }
        string BinDir { get; }
        string ModuleName { get; }
    }
}
