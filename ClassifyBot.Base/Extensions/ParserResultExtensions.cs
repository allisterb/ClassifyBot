using System;
using System.Collections.Generic;
using System.Text;

using Serilog;

using CommandLine;

namespace ClassifyBot
{
    public static class ParserResultExtensions
    {
        private static ILogger L = Log.ForContext<ParserResult<object>>();
        public static ParserResult<object> WithParsed(this ParserResult<object> result, Type t, Action<object> action)
        {
            Parsed<object> parsed = result as Parsed<object>;
            if (parsed != null)
            {
                if (parsed.Value.GetType() == t)
                {
                    action(parsed.Value);
                }
            }
            return result;
        }
    }
}
