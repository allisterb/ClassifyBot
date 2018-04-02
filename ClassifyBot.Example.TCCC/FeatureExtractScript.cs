using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Python.Runtime;
using Serilog;
using SerilogTimings;

namespace ClassifyBot.Example.TCCC
{
    public class FeatureExtractScript : PythonScript
    {
        #region Constructors
        public FeatureExtractScript(string homeDir, List<Comment> comments = null) : base(homeDir)
        {
            RequiredModules = new List<string> { "nltk", "vaderSentiment"};
            if (comments != null)
            {
                this.Comments = comments;
            }
                        
        }
        #endregion

        #region Overriden members
        public override bool Init()
        {
            if (!base.Init()) return false;
            globals = new PyDict();
            commentsDict = new PyDict();
            globals.SetItem("nltk", Py.Import("nltk"));
            globals.SetItem("vader", Py.Import("vaderSentiment.vaderSentiment"));
            globals.SetItem("comments", commentsDict);
            
            return true;
        }

        public override bool Run()
        {
            if (!TokenizeComments()) return false;
            if (!CalculateSentiment()) return false;
            foreach (Comment c in comments)
            {
                c.Features.Add(("WORDS", commentsWords[c._Id.Value].As<string>()));
                c.Features.Add(("SENTIMENT", commentsSentiment[c._Id.Value]["compound"].As<float>().ToString()));
            }
            return true;
        }
        #endregion

        #region Properties
        public List<Comment> Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                foreach (Comment c in comments)
                {
                    commentsDict.SetItem(c._Id.Value, new PyString(c.Features[0].Item2));
                }
            }
        }
        #endregion

        #region Methods
        protected bool TokenizeComments()
        {
            Operation tokenize = Begin("Tokenizing {0} comments using {1}", commentsDict.Length(), "NLTK word tokenizer");
            using (Py.GIL())
            {
                PyObject r = PythonEngine.Eval("{k: '<->'.join(nltk.word_tokenize(v)) for k, v in comments.items()}", globals.Handle);
                commentsWords = new PyDict(r.Handle);
            }
            tokenize.Complete();
            return true;
        }

        protected bool CalculateSentiment()
        {
            Operation sentiment = Begin("Calculating sentiment scores for {0} comments using {1}.", commentsDict.Length(), "VADER sentiment analyzer");

            using (Py.GIL())
            {
                PyObject analyzer = PythonEngine.Eval("vader.SentimentIntensityAnalyzer()", globals.Handle);
                globals["analyzer"] = analyzer;
                PyObject r = PythonEngine.Eval("{k: analyzer.polarity_scores(v) for k, v in comments.items()}", globals.Handle);
                commentsSentiment = new PyDict(r.Handle);
                sentiment.Complete();
            }
            return true;
        }
        #endregion

        #region Fields
        protected List<Comment> comments;
        protected PyDict commentsDict;
        protected PyDict commentsWords;
        protected PyDict commentsSentiment;
        #endregion
    }
}
