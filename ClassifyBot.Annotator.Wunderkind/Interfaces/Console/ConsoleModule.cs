using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses.Module;

namespace ClassifyBot.Annotator.Wunderkind
{
    public abstract class ConsoleModule : Module
    {
        #region Overriden members
        public override void OnTick(bool systemTick, bool skipDay = false)
        {
            base.OnTick(systemTick, skipDay);

            // Skip system ticks.
            if (systemTick)
            {
                return;
            }
        }
        #endregion

        

        #region Methods
        public abstract void Restart();
        #endregion
    }
}
