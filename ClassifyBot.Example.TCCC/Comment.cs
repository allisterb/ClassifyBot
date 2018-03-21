using System;

namespace ClassifyBot.Example.TCCC
{
    public class Comment : Record<string>
    {
        #region Constructors
        public Comment(int lineNo, string id, string text, int toxic = 0, int severe_toxic = 0, int obscene = 0, int threat = 0, int insult = 0, int identity_hate = 0) 
            : base(lineNo, id, ("toxic", toxic), ("severe_toxic", severe_toxic), ("obscene", obscene), ("threat", threat), ("insult", insult), ("identity_hate", identity_hate))
        {
            this.Features.Add(("TEXT", text));
            this.toxic = toxic;
            this.severe_toxic = severe_toxic;
            this.obscene = obscene;
            this.threat = threat;
            this.insult = insult;
            this.identity_hate = identity_hate;
        }

        public Comment(Comment c)
        {
            this._Id = c._Id;
            this.Id = c.Id;
            foreach ((string, float) l in c.Labels)
            {
                this.Labels.Add((l.Item1, l.Item2));
            }
            foreach ((string, string) f in c.Features)
            {
                this.Features.Add((f.Item1, f.Item2));
            }
            this.toxic = c.toxic;
            this.severe_toxic = c.severe_toxic;
            this.obscene = c.obscene;
            this.threat = c.threat;
            this.insult = c.insult;
            this.identity_hate = c.identity_hate;

        }

        public Comment() {}
        #endregion

        #region Fields
        internal int toxic;
        internal int severe_toxic;
        internal int obscene;
        internal int threat;
        internal int insult;
        internal int identity_hate;
        #endregion
    }
}
