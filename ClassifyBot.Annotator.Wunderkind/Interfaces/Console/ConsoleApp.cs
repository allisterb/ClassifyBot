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
            this.SceneGraph2 = new SceneGraph2(this);
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
            
            // No ticks allowed if simulation is shutting down.
            if (IsClosing)
                return;

            // Sends commands if queue has any.
            InputManager?.OnTick(systemTick, skipDay);

            // Back buffer for only sending text when changed.
            SceneGraph2?.OnTick(systemTick, skipDay);

            // Changes game Windows and state when needed.
            WindowManager?.OnTick(systemTick, skipDay);

            // Rolls virtual dice.
            Random?.OnTick(systemTick, skipDay);

            // System tick is from execution platform, otherwise they are linear simulation ticks.
            if (systemTick)
            {
                // Recursive call on ourselves to process non-system ticks.
                OnTick(false, skipDay);
            }
            else
            {
                //Any actions
            }
            
            //base.OnTick(systemTick, skipDay);

            // Tick each module
            foreach (ConsoleModule m in Modules)
            {
                m.OnTick(systemTick, skipDay);
            }
            
        }

        public override void Restart()
        {
            base.Restart();
            SceneGraph2.Clear();
        }

        public override void Destroy()
        {
            base.Destroy();
            SceneGraph2.Clear();
        }
        #endregion

        #region Properties
        public SceneGraph2 SceneGraph2 { get; protected set; }

        public List<ConsoleModule> Modules { get; protected set; } = new List<ConsoleModule>();

        public string Title { get; protected set; }

        protected Annotator<TRecord, TFeature> Annotator { get; set; }

        protected static ILogger L = Log.Logger.ForContext<ConsoleApp<TRecord, TFeature>>();
        #endregion

        #region Methods
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

        public virtual StageResult Init()
        {
            SetPropFromDict(typeof(ConsoleApp<TRecord, TFeature>), this, Annotator.InterfaceOptions);
            this.Modules.Add(new StatusModule());
            this.SceneGraph2.ScreenBufferDirtyEvent += SceneGraph_ScreenBufferDirtyEvent;
            L.Information("Initialized console application for annotator.");
            return StageResult.SUCCESS;
        }

        public virtual StageResult Run()
        {
            string oldTitle = Console.Title;
            Console.Title = Title;
            Console.CursorVisible = false;
            Console.Clear();
            Console.CancelKeyPress += Console_CancelKeyPress;
            Tick();
            Console.CursorVisible = true;
            Console.Title = oldTitle;
            if (CtrlCRequested)
            {
                return StageResult.ABORTED;
            }
            return StageResult.SUCCESS;
        }

        protected virtual void Tick()
        {
            lastSystemTickTime = DateTime.UtcNow;
            while (!this.IsClosing)
            {
                currentSystemTickTime = DateTime.UtcNow;
                // Check if more than an entire second has gone by.
                if ((TimeSpan.FromTicks(currentSystemTickTime.Ticks - lastSystemTickTime.Ticks).TotalMilliseconds < (1000 / fps)))
                {
                    Thread.Sleep(5);
                }
                else
                {
                    lastSystemTickTime = currentSystemTickTime;
                    this.OnTick(true);
                }
                ReadAndDispatchConsoleKeys();
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
                    case ConsoleKey.Escape:
                        L.Information("Stopping");
                        this.Destroy();
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
            L.Error("Ctrl-C abort requested. Terminating console application");
            e.Cancel = true;
            CtrlCRequested = true;
            this.Destroy();
        }
        #endregion

        #region Fields
        protected DateTime currentSystemTickTime;

        protected DateTime lastSystemTickTime;

        protected ulong elapsedSeconds;

        protected int fps = 60;

        protected bool CtrlCRequested;
        #endregion
    }
}
