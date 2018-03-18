using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public class ClassifierResult : IClassifierResult
    {
        #region Properties
        public int _Id { get; set; }
        public string Id { get; set; }
        public string ClassifierAnswer { get; set; }
        public string GoldAnswer { get; set; }
        public float P_ClAnswer { get; set; }
        public float P_GoldAnswer { get; set; }
        #endregion
    }
}
