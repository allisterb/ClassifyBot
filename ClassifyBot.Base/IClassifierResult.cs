using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public interface IClassifierResult
    {
        int _Id { get; set; }
        string Id { get; set; }
        string ClassifierAnswer { get; set; }
        string GoldAnswer { get; set; }
        float P_ClAnswer { get; set; }
        float P_GoldAnswer { get; set; }
    }
}
