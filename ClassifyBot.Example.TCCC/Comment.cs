using System;

namespace ClassifyBot.Example.TCCC
{
    public class Comment : Record<string>
    {
        public Comment(int lineNo, string id, string text, int toxic = 0, int severe_toxic = 0, int obscene = 0, int threat = 0, int insult = 0, int identity_hate = 0) 
            : base(lineNo, id, ("toxic", toxic), ("severe_toxic", severe_toxic), ("obscene", obscene), ("threat", threat), ("insult", insult), ("identity_hate", identity_hate))
        {
            this.Features.Add(("TEXT", text));
        }
    }
}
