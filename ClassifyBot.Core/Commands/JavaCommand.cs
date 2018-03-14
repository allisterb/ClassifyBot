using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot.Commands
{
    public class JavaCommand : Command
    {
        #region Constructors
        public JavaCommand(string classPath, params object[] options) : base("java", options)
        {
            this.ClassPath = classPath;
            if (Java_Home_Exists)
            {
                string javaCommand = Path.Combine(Java_Home, "bin", "java");
                CommandText = "{0} -cp {1} {2}".F(javaCommand, ClassPath, CommandOptions).Trim();
            }
            else
            {
                string javaCommand = "java";
                CommandText = "{0} -cp {1} {2}".F(javaCommand, ClassPath, CommandOptions).Trim();
            }
        }

        public JavaCommand(string java_home, string classPath, params object[] options) : this("java", options)
        {
            _Java_Home = java_home;
            string javaCommand = Path.Combine(Java_Home, "bin", "java");
            CommandText = "{0} -cp {1} {2}".F(javaCommand, ClassPath, CommandOptions).Trim();

        }
        #endregion

        #region Properties
        public static string Java_Home
        {
            get
            {
                if (_Java_Home.Empty())
                {
                    _Java_Home = Environment.GetEnvironmentVariable("JAVA_HOME");
                }
                return _Java_Home;
            }
            set
            {
                _Java_Home = value;
            }
        }

        public static bool Java_Home_Exists => Java_Home != null ? Directory.Exists(Java_Home) : false;

        public string ClassPath { get; protected set; } 
        #endregion

        #region Fields
        private static ILogger L = Log.ForContext<JavaCommand>();
        private static string _Java_Home;
        #endregion
    }
}
    
