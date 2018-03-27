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
        public FeatureExtractScript(string homeDir) : base(homeDir)
        {
            RequiredModules = new List<string> { "nltk" };
        }

        public bool TokenizeComments(List<Comment> comments)
        {
            Operation count = Begin("Tokenizing {0} comments using NLTK word tokenizer", comments.Count);
            
            Dictionary<int, string> _sentences = comments.ToDictionary(c => c._Id.Value, c => c.Features[0].Item2);
            using (Py.GIL())
            using (PyDict g = new PyDict())
            using (PyDict sentences = new PyDict())
            {
                foreach(Comment c in comments)
                {
                    sentences.SetItem(c._Id.Value, new PyString(c.Features[0].Item2));
                }
                g.SetItem("nltk", Py.Import("nltk"));
                g.SetItem("sentences", sentences);
                PyObject r = PythonEngine.Eval("{k: '<->'.join(nltk.word_tokenize(v)) for k, v in sentences.items()}", g.Handle);
                PyDict d = new PyDict(r.Handle);
                foreach (Comment c in comments)
                {
                    c.Features.Add(("WORDS", d[c._Id.Value].As<string>()));
                }
            }
            count.Complete();
            return true;
        }

    }
}
