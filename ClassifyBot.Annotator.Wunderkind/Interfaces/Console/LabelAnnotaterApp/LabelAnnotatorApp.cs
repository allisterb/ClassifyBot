using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses.Utility;

namespace ClassifyBot.Annotator.Wunderkind
{
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
                List<Type> windowList = new List<Type>
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
            // Resets the window manager in the base simulation.
            base.Restart();

            // Attach example window after the first tick.
            WindowManager.Add(typeof(LabelAnnotatorWindow));
        }
        #endregion
    }
}
