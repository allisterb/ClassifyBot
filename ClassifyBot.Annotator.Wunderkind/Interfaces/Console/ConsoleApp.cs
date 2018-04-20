using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

using Serilog;
using SerilogTimings;

using Console = Colorful.Console;
using Colorful;
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
            fonts.Add("chunky", FigletFont.Load(Path.Combine(EntryAssemblyDirectory.FullName, "Interfaces", "Console", "Fonts", "chunky.flf")));
            figlet.Add("chunky", new Figlet(fonts["chunky"]));
            preprocessRegex = new Regex("<@(.+?)@>", RegexOptions.Compiled | RegexOptions.Multiline);
        }
        #endregion      

        #region Properties
        public SceneGraph2 SceneGraph2 { get; protected set; }

        public List<ConsoleModule> Modules { get; protected set; } = new List<ConsoleModule>();

        public static DirectoryInfo EntryAssemblyDirectory { get; } = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

        public string Title { get; protected set; }

        public int ConsoleWidth { get; protected set; } = 150;

        public int ConsoleHeight { get; protected set; } = 40;

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
            int oldWidth = Console.WindowWidth;
            int oldHeight = Console.WindowWidth;
            Console.WindowWidth = ConsoleWidth;
            Console.WindowHeight = ConsoleHeight;
            Console.CursorVisible = false;
            Console.Clear();
            Console.CancelKeyPress += Console_CancelKeyPress;
            Tick();
            Console.CursorVisible = true;
            Console.Title = oldTitle;
            Console.WindowWidth = oldWidth;
            Console.WindowHeight = oldHeight;
            if (CtrlCRequested)
            {
                return StageResult.ABORTED;
            }
            return StageResult.SUCCESS;
        }

        protected virtual void Tick()
        {
            lastSystemTickTime = DateTime.UtcNow;
            double stepInterval;
            while (!this.IsClosing)
            {
                currentSystemTickTime = DateTime.UtcNow;
                // Check if more than an entire frame time by.
                stepInterval = TimeSpan.FromTicks(currentSystemTickTime.Ticks - lastSystemTickTime.Ticks).TotalMilliseconds;
                if ((stepInterval < (1000 / fps)))
                {
                    Thread.Sleep((int) (stepInterval / 2));
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

        //protected void Parse
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
                if (TotalFramesElapsed == 0)
                {
                    OnFirstTick();
                }
            }
            // Tick each module
            foreach (ConsoleModule m in Modules)
            {
                m.OnTick(systemTick, skipDay);
            }

            lastFrameRenderTime = DateTime.UtcNow;
            TotalFramesElapsed++;
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

            TotalFramesElapsed = 0;
            startTime = DateTime.UtcNow;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SceneGraph_ScreenBufferDirtyEvent(string buffer)
        {
            string[] lines = buffer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
       
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Match m = preprocessRegex.Match(line);
                if (!m.Success)
                {
                    Console.WriteLine(line);
                }
                else
                {
                    string[] command = m.Groups[1].Value.Split('|');
                    string font = command[0];
                    string text = command[1];
                    string fg = command[2];
                    string bg = command[3];
                    StyledString s;
                    if (command[0].IsNotEmpty())
                    {
                        //Console.WriteLine(figlet[font].ToAscii(text));
                        Console.WriteAscii(text);
                    }
                    else
                    {
                        Console.WriteAscii(text);
                    }
                }
            }
        }

        private StyledString[] PreProcessBuffer(string buffer)
        {
            string output = string.Empty;
            List<StyledString> lines = new List<StyledString>();
            Match m = preprocessRegex.Match(buffer);
            if (!m.Success)
            {
                return buffer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(s => new StyledString(s)).ToArray();
            }

            for(int i = 1; i < m.Groups.Count; i++)
            {
                string[] command = m.Groups[i].Value.Split('|');
                string font = command[0];
                string text = command[1];
                string fg = command[2];
                string bg = command[2];
                if (command[0].IsNotEmpty())
                {
                    var s = figlet[font].ToAscii(text);
                }
                    
            }
            return null;
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
        protected DateTime startTime;

        protected DateTime currentSystemTickTime;

        protected DateTime lastSystemTickTime;

        protected DateTime lastFrameRenderTime;

        protected ulong _TotalFramesElapsed;

        protected ulong elapsedSeconds;

        protected int fps = 60;

        protected bool CtrlCRequested;

        protected Dictionary<string, FigletFont> fonts = new Dictionary<string, FigletFont>();

        protected Dictionary<string, Figlet> figlet = new Dictionary<string, Figlet>();

        protected Regex preprocessRegex;
        #endregion
    }
}
