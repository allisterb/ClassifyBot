using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public class ClassStatistic : IClassStatistic
    {
        #region Properties
        public string Name { get; set; }
        public int TruePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalseNegatives { get; set; }
        public int FalsePositives { get; set; }
        public float Accuracy { get; set; }
        public float Precision { get; set; }
        public float Recall { get; set; }
        public float F1 { get; set; }
        #endregion
    }
}
