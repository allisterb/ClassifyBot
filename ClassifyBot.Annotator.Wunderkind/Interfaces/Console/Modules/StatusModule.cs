using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot.Annotator.Wunderkind
{
    public class StatusModule : ConsoleModule
    {
        #region Overriden members
        public override void OnTick(bool systemTick, bool skipDay = false)
        {
            base.OnTick(systemTick, skipDay);

        }
        public override void Restart()
        {
            
        }
        #endregion

        #region Properties
        public string Time { get; protected set; }
        #endregion
    }
}
