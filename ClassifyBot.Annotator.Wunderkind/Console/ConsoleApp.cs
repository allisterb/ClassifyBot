using System;
using System.Collections.Generic;
using System.Text;

using WolfCurses;
using WolfCurses.Module;

namespace ClassifyBot.Annotator.Wunderkind
{
    public class ConsoleApp : SimulationApp
    {
        #region Overriden methods
        public override IEnumerable<Type> AllowedWindows
        {
            get
            {
                var windowList = new List<Type>
                {
                    typeof (ConsoleWindow)
                };

                return windowList;
            }
        }

        /// <summary>
        ///     Fired when the ticker receives the first system tick event.
        /// </summary>
        protected override void OnFirstTick()
        {
            Restart();
        }

        /// <summary>
        ///     Called when simulation is about to destroy itself, but right before it actually does it.
        /// </summary>
        protected override void OnPreDestroy()
        {
            // Allows module to save any data before core simulation closes.
            foreach(ConsoleModule m in Modules)
            {
                if (m != null)
                {
                    m.Destroy();
                }
            }

            // Destroys simulation instance.
            Instance = null;
        }

        /// <summary>
        ///     Called by the text user interface scene graph renderer before it asks the active window to render itself out for
        ///     display.
        /// </summary>
        public override string OnPreRender()
        {
            // Total number of turns that have passed in the simulation.
            var tui = new StringBuilder();
            
            return tui.ToString();
        }

        /// <summary>
        ///     Called when the simulation is ticked by underlying operating system, game engine, or potato. Each of these system
        ///     ticks is called at unpredictable rates, however if not a system tick that means the simulation has processed enough
        ///     of them to fire off event for fixed interval that is set in the core simulation by constant in milliseconds.
        /// </summary>
        /// <remarks>Default is one second or 1000ms.</remarks>
        /// <param name="systemTick">
        ///     TRUE if ticked unpredictably by underlying operating system, game engine, or potato. FALSE if
        ///     pulsed by game simulation at fixed interval.
        /// </param>
        /// <param name="skipDay">
        ///     Determines if the simulation has force ticked without advancing time or down the trail. Used by
        ///     special events that want to simulate passage of time without actually any actual time moving by.
        /// </param>
        public override void OnTick(bool systemTick, bool skipDay = false)
        {
            base.OnTick(systemTick, skipDay);

            // Tick each module
            foreach (ConsoleModule m in Modules)
            {
                m.OnTick(systemTick, skipDay);
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
            WindowManager.Add(typeof(ConsoleWindow));
        }
        #endregion

        #region Properties
        public static ConsoleApp Instance { get; private set; }
        public List<ConsoleModule> Modules { get; protected set; }
        #endregion
    }
}
