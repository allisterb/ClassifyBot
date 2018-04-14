using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses;
using WolfCurses.Window;

namespace ClassifyBot.Annotator.Wunderkind
{
    public abstract class ConsoleWindow<TCommands, TData> : Window<TCommands, TData> where TCommands : struct, IComparable, IFormattable, IConvertible
        where TData : WindowData, new()
    {
        #region Constructors
        public ConsoleWindow(SimulationApp app) : base(app)
        {
        }
        #endregion
    }
}
