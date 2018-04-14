using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses.Utility;

namespace ClassifyBot.Annotator.Wunderkind
{
    #region Enums
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
    #endregion

    public class LabelAnnotatorApp<TRecord, TFeature> : ConsoleApp<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public LabelAnnotatorApp(Annotator<TRecord, TFeature> annotator) : base(annotator) {}
        #endregion

        #region Overriden members
        public override IEnumerable<Type> AllowedWindows
        {
            get
            {
                var windowList = new List<Type>
                {
                    typeof (LabelAnnotatorWindow)
                };

                return windowList;
            }
        }

        /// <summary>
        ///     Creates and or clears data sets required for game simulation and attaches the travel menu and the main menu to make
        ///     the program completely restarted as if fresh.
        /// </summary>
        public override void Restart()
        {
            // Resets the module to default start.
            foreach (ConsoleModule m in Modules)
            {
                m.Restart();
            }

            // Resets the window manager in the base simulation.
            base.Restart();

            // Attach example window after the first tick.
            WindowManager.Add(typeof(LabelAnnotatorWindow));
        }
        #endregion
    }
}
