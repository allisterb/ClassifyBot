using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassifyBot
{
    public interface IReporter 
    {
        StageResult Run(Dictionary<string, object> options = null);
    }
}
