using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses;
namespace ClassifyBot.Annotator.Wunderkind
{
    public class LabelAnnotatorWindow : ConsoleWindow<LabelAnnotatorAppCommand, ConsoleWindowData>
    {
        #region Constructors
        public LabelAnnotatorWindow(SimulationApp app) : base(app)
        {
        }
        #endregion

        #region Overriden members
        /// <summary>
        ///     Called after the Windows has been added to list of modes and made active.
        /// </summary>
        public override void OnWindowPostCreate()
        {
            base.OnWindowPostCreate();

            var headerText = new StringBuilder();
            headerText.Append(
                $"{Environment.NewLine}Example Console Application{Environment.NewLine}{Environment.NewLine}");
            headerText.Append("You may:");
            MenuHeader = headerText.ToString();
            /*
            AddCommand(TextPrompt, ExampleCommands.TextPrompt);
            AddCommand(YesNoPrompt, ExampleCommands.YesNoPrompt);
            AddCommand(CustomPrompt, ExampleCommands.CustomPrompt);
            AddCommand(CloseSimulation, ExampleCommands.CloseSimulation);
            */
        }
        #endregion
    }
}
