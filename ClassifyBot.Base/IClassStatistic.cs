using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public interface IClassStatistic
    {
        int TruePositives { get; }
        int TrueNegatives { get; }
        int FalseNegatives { get; }
        int FalsePositives { get; }
        float Accuracy { get; }
        float Precision { get; }
        float Recall { get; }
        float F1 { get; }
    }
}
