using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace ClassifyBot
{
    [Verb("extract", HelpText = "Extract records from a data source into a common JSON format.")]
    public abstract class ExtractStage<TRecord, TFeature> : Stage, IExtractor<TRecord, TFeature> 
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public ExtractStage()
        {
            Contract.Requires(!OutputFileName.Empty());
            L = Log.ForContext<ExtractStage<TRecord, TFeature>>();
            if (!CompressOutputFile)
            {
                if (AppendToOutputFile && JsonOutputFile.Exists)
                {
                    using (StreamReader f = new StreamReader(JsonOutputFile.OpenRead()))
                    using (JsonTextReader reader = new JsonTextReader(f))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ExtractedRecords = serializer.Deserialize<List<TRecord>>(reader);
                        L.Information("{file} exists with {r} records and will be appended to.", JsonOutputFile, ExtractedRecords.Count);
                    }
                }
            }
            else
            {
                if (AppendToOutputFile && JsonOutputFile.Exists)
                {
                    using (GZipStream gzs = new GZipStream(JsonOutputFile.OpenRead(), CompressionMode.Decompress))
                    using (StreamReader f = new StreamReader(gzs))
                    using (JsonTextReader reader = new JsonTextReader(f))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ExtractedRecords = serializer.Deserialize<List<TRecord>>(reader);
                        L.Information("{file} exists with {r} records and will be appended to.", JsonOutputFile, ExtractedRecords.Count);
                    }
                }
            }
        }
        #endregion

        #region Abstract methods
        public abstract int Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null);
        #endregion

        #region Properties
        public FileInfo JsonOutputFile => OutputFileName.Empty() ? null : new FileInfo(OutputFileName);

        public List<TRecord> ExtractedRecords { get; protected set; } = new List<TRecord>();

        public int RecordLimit { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for dataset. A file with .json or .json.gz extension will be created with this name.")]
        public string OutputFileName { get; set; }

        [Option('o', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output data file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('a', "append", Required = false, Default = false, HelpText = "Append extracted data to existing output file if it exists.")]
        public bool AppendToOutputFile { get; set; }

        [Option('c', "compress", Required = false, Default = false, HelpText = "Output file will be compressed with GZIP.")]
        public bool CompressOutputFile { get; set; }

        [Option('b', "batch", Required = false, HelpText = "Batch the number of records extracted.", Default = 0)]
        public int RecordBatchSize { get; set; }

        [Option('l', "records", Required = false, HelpText = "Limit the number of records extracted.", Default = 0)]
        
        protected virtual ILogger L { get; } = Log.ForContext<ExtractStage<TRecord, TFeature>>();
        #endregion

        #region Methods
        public virtual bool Save()
        {
            Contract.Requires(ExtractedRecords != null);
            Contract.Requires(ExtractedRecords.Count > 0);
            Contract.Requires(JsonOutputFile != null && JsonOutputFile.Exists);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if (!CompressOutputFile)
            {
                using (StreamWriter sw = new StreamWriter(JsonOutputFile.FullName, false, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(JsonOutputFile.FullName, FileMode.Create))
                using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gzs, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            L.Information("Wrote {0} records to {file}", ExtractedRecords.Count, JsonOutputFile.FullName);
            return true;
        }
        protected static string FilterNonText(string t)
        {
            string text = Regex.Replace(t, @"[\u000A\u000B\u000C\u000D\u2028\u2029\u0085]+", String.Empty); //filter out line terminators
            text = Regex.Replace(text, @"\s+", " "); //compress multiple white space into 1
            text = Regex.Replace(text, @"http[^\s]+", ""); //filter out urls
            text = Regex.Replace(text, @"\[^\s+\]", ""); //filter out [text]
            return new string(text.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()); //filter out anything non-alphanumeric
        }
        #endregion
    }
}
