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

        #region Properties
        public SceneGraph2 SceneGraph2 { get; protected set; }

        public List<ConsoleModule> Modules { get; protected set; } = new List<ConsoleModule>();

        public string Title { get; protected set; }

        protected Annotator<TRecord, TFeature> Annotator { get; set; }

        protected static ILogger L = Log.Logger.ForContext<ConsoleApp<TRecord, TFeature>>();
        
        protected ulong TotalFramesElapsed
        {
            get
            {
                if (_TotalFramesElapsed == UInt64.MaxValue)
                {
                    _TotalFramesElapsed = 0;
                }
                return _TotalFramesElapsed;
            }
            set
            {
                _TotalFramesElapsed = value;
            }
        }
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
                // Check if more than an entire frame time by.
                if ((TimeSpan.FromTicks(currentSystemTickTime.Ticks - lastSystemTickTime.Ticks).TotalMilliseconds < (1000 / fps)))
                {
                    Thread.Sleep(5);
                }
                else
                {
                    lastSystemTickTime = currentSystemTickTime;
                    this.OnTick(true);
                    ReadAndDispatchConsoleKeys();
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

        #region Overriden methods
        public override string OnPreRender() // Gather data from modules
        {
            return string.Empty;
        }

        public override void OnTick(bool systemTick, bool skipDay = false)
        {
            if (IsClosing)
            {
                return;
            }
            InputManager?.OnTick(systemTick, skipDay);

            SceneGraph2?.OnTick(systemTick, skipDay);

            WindowManager?.OnTick(systemTick, skipDay);

            Random?.OnTick(systemTick, skipDay);

            if (systemTick)
            {
                OnTick(false, skipDay);
            }
            else
            {
                TotalFramesElapsed++;
                if (TotalFramesElapsed == 1)
                {
                    OnFirstTick();
                }
            }
            // Tick each module
            foreach (ConsoleModule m in Modules)
            {
                m.OnTick(systemTick, skipDay);
            }
        }

        protected override void OnFirstTick()
        {
            Restart();
        }

        public override void Restart()
        {
            // Resets the window manager in the base simulation.
            base.Restart();
        
            SceneGraph2.Clear();

            // Resets the module to default start.
            foreach (ConsoleModule m in Modules)
            {
                m.Restart();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            SceneGraph2 = null;
        }

        

        protected override void OnPreDestroy()
        {
            foreach (ConsoleModule m in Modules)
            {
                if (m != null)
                {
                    m.Destroy();
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

        protected ulong _TotalFramesElapsed;

        protected ulong elapsedSeconds;

        protected int fps = 60;

        protected bool CtrlCRequested;
        #endregion
    }
}
