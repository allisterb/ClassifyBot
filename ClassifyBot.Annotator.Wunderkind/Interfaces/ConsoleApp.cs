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

namespace ClassifyBot.Annotator.Wunderkind
{
    public abstract class ConsoleApp2<TRecord, TFeature> : IAnnotatorInterface<TRecord, TFeature> 
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> 
        where TRecord : Record<TFeature>
    {
        #region Constructors
        public ConsoleApp2(Annotator<TRecord, TFeature> annotator)
        {
            
        }
        #endregion      

        #region Properties
        
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
           
            L.Information("Initialized console application for annotator.");
            return StageResult.SUCCESS;
        }

        public virtual StageResult Run()
        {
           
            return StageResult.SUCCESS;
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

        protected string buffer;

        protected StyledString[] bufferLines;

        protected StyledString[] oldBufferLines = new StyledString[0];

        protected Regex preprocessRegex;
        #endregion
    }
}
