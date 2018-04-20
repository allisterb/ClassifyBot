using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses.Window;
using WolfCurses.Window.Form;

namespace ClassifyBot.Annotator.Wunderkind
{
    [ParentWindow(typeof(LabelAnnotatorWindow))]
    public class LabelAnnotatorTitleForm : Form<ConsoleWindowData>
    {
        #region Constructors
        public LabelAnnotatorTitleForm(IWindow window) : base(window)
        {

        }
        #endregion

        #region Overriden members
        public override string OnRenderForm()
        {
            return "<@chunky|Wunderkind|#8AFFEF||@>";

        }

        public override void OnInputBufferReturned(string input)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
