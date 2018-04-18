using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses.Utility;

namespace ClassifyBot.Annotator.Wunderkind
{
    public enum LabelAnnotatorAppCommand
    {
        /// <summary>
        ///     Basic
        /// </summary>
        [Description("Prompts with text and no input.")] TextPrompt = 1,

        /// <summary>
        ///     Dialog prompt that asks a YES or NO question, user can enter single letters, spell it out, or even use alternative
        ///     words like NOPE.
        /// </summary>
        [Description("Prompts with yes/no question.")] YesNoPrompt = 2,

        /// <summary>
        ///     Dialog prompt that is not a question but waiting for specific information such as the users name.
        /// </summary>
        [Description("Prompts with custom input required.")] CustomPrompt = 3,

        /// <summary>
        ///     Closes the console application.
        /// </summary>
        [Description("End")] CloseSimulation = 4
    }
}
