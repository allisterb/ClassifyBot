using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

using Serilog;
using SerilogTimings;

using WolfCurses;
using WolfCurses.Module;

namespace ClassifyBot.Annotator.Wunderkind
{
    public abstract class ConsoleApp<TRecord, TFeature> : SimulationApp, IAnnotatorInterface<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public ConsoleApp(Annotator<TRecord, TFeature> annotator) : base()
        {
            Annotator = annotator;
        }
        #endregion      

        #region Overriden methods
      

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
        #endregion

        #region Properties
        public static ConsoleApp<TRecord, TFeature> Instance { get; private set; }

        public List<ConsoleModule> Modules { get; protected set; } = new List<ConsoleModule>();

        public string Title { get; protected set; }

        protected Annotator<TRecord, TFeature> Annotator { get; set; }

        protected static ILogger L = Log.Logger.ForContext<ConsoleApp<TRecord, TFeature>>();
        #endregion

        #region Methods
        public virtual StageResult Run()
        {
            Console.Title = Title;
            Console.CursorVisible = false;
            Console.CancelKeyPress += Console_CancelKeyPress;

            bool close = false;
            // Prevent console session from closing.
            Console.Clear();

            while (!this.IsClosing)
            {
                // Simulation takes any numbers of pulses to determine seconds elapsed.
                this.OnTick(true);
                ReadAndDispatchConsoleKeys();

            }
            return StageResult.SUCCESS;
        }

        public virtual StageResult Init()
        {
            SetPropFromDict(typeof(ConsoleApp<TRecord, TFeature>), this, Annotator.InterfaceOptions);
            this.Modules.Add(new StatusModule());
            this.SceneGraph.ScreenBufferDirtyEvent += SceneGraph_ScreenBufferDirtyEvent;
            L.Information("Initialized console application for annotator.");
            return StageResult.SUCCESS;
        }

        internal static void SetPropFromDict(Type t, object o, Dictionary<string, object> p)
        {
            foreach (PropertyInfo prop in t.GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(o, p[prop.Name]);
                }
            }
        }
        
        protected void ReadAndDispatchConsoleKeys()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        this.InputManager.SendInputBufferAsCommand();
                        break;
                    case ConsoleKey.Backspace:
                        this.InputManager.RemoveLastCharOfInputBuffer();
                        break;
                    default:
                        this.InputManager.AddCharToInputBuffer(key.KeyChar);
                        break;
                }
            }
        }
        #endregion

        #region Event handlers
        private void SceneGraph_ScreenBufferDirtyEvent(string tuiContent)
        {
            string[] tuiContentSplit = tuiContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int index = 0; index < Console.WindowHeight - 1; index++)
            {
                Console.CursorLeft = 0;
                Console.SetCursorPosition(0, index);

                string emptyStringData = new string(' ', Console.WindowWidth);

                if (tuiContentSplit.Length > index)
                {
                    emptyStringData = tuiContentSplit[index].PadRight(Console.WindowWidth);
                }

                Console.Write(emptyStringData);
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Fields
        /// <summary>
        ///     Time and date of latest system tick, used to measure total elapsed time and tick simulation after each second.
        /// </summary>
        protected DateTime currentSystemTickTime;

        /// <summary>
        ///     Last known time the simulation was ticked with logic and all sub-systems. This is not the same as a system tick
        ///     which can happen hundreds of thousands of times a second or just a few, we only measure the difference in time on
        ///     them.
        /// </summary>
        protected DateTime lastSystemTickTime;

        protected long currentFrameElapsedTicks;

        protected TimeSpan currentFrameElapsedInterval;

        protected int fps = 60;

        protected float frameInterval = 1000 / 60;


        #endregion
    }
}
