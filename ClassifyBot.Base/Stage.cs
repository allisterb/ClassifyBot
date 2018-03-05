using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ClassifyBot
{
    public abstract class Stage
    {
        #region Constructors
        

        public Stage()
        {
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                if (!StageOptions.ContainsKey(prop.Name))
                {
                    StageOptions.Add(prop.Name, prop.GetValue(this));
                }
            }
        }
        #endregion

        #region Properties
        public static Dictionary<string, object> StageOptions { get; } = new Dictionary<string, object>();
        #endregion

    }
}
