using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using SerilogTimings;


namespace ClassifyBot
{
    public abstract class Transformer<TRecord, TFeature> : Stage, ITransformer<TRecord, TFeature>
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public Transformer() : base()
        {
            Contract.Requires(!InputFileName.IsEmpty());
            Contract.Requires(!OutputFileName.IsEmpty());
        }
        #endregion

        #region Overriden members
        public override StageResult Run(Dictionary<string, object> options = null)
        {
            StageResult r;
            if ((r = Init()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Read()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Transform()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Write()) != StageResult.SUCCESS)
            {
                return r;
            }
            Cleanup();
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            Contract.Requires(InputFile != null && OutputFile == null);
            if (!InputFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (OutputFile.Exists && !OverwriteOutputFile)
            {
                Error("The output file {0} exists but the overwrite option was not specified.", OutputFile.FullName);
                return StageResult.OUTPUT_ERROR;
            }
            else if (OutputFile.Exists)
            {
                Warn("Output file {0} exists and will be overwritten.", OutputFile.FullName);
            }

            return StageResult.SUCCESS;
        }

        protected override StageResult Read()
        {
            Info("Deserializing records from {0}...", InputFile.FullName);
            if (InputFile.Extension == ".gz")
            {
                using (GZipStream gzs = new GZipStream(InputFile.OpenRead(), CompressionMode.Decompress))
                using (StreamReader r = new StreamReader(gzs))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    InputRecords = serializer.Deserialize<List<TRecord>>(reader);
                }
            }
            else
            {
                using (StreamReader r = new StreamReader(InputFile.OpenRead()))
                using (JsonTextReader reader = new JsonTextReader(r))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    InputRecords = serializer.Deserialize<List<TRecord>>(reader);
                }
            }
            if (InputRecords == null || InputRecords.Count == 0)
            {
                Error("Did not read any records from {0}.", InputFile.FullName);
                return StageResult.INPUT_ERROR;
            }
            else
            {
                Info("Read {0} records from {1}.", InputRecords.Count, InputFile.FullName);
                return StageResult.SUCCESS;
            }
        }

        protected override StageResult Process() => Transform();

        protected override StageResult Write()
        {
            Contract.Requires(OutputRecords != null);
            if (OutputRecords.Count == 0)
            {
                Warn("0 records transformed from file {0}. Not writing to output file.", InputFile.FullName);
                return StageResult.SUCCESS;
            }
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if (!CompressOutputFile)
            {
                using (FileStream fs = new FileStream(OutputFile.FullName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    serializer.Serialize(sw, OutputRecords);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(OutputFile.FullName, FileMode.Create))
                using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gzs, Encoding.UTF8))
                {
                    serializer.Serialize(sw, OutputRecords);
                }
            }
            if (!CompressOutputFile)
            {
                Info("Wrote {0} output records to {1}.", OutputRecords.Count, OutputFileName);
            }
            else
            {
                Info("Wrote {0} output records to gzip-compressed {1}.", OutputRecords.Count, OutputFileName);
            }
            return StageResult.SUCCESS;
        }
        #endregion

        #region Abstract members
        protected abstract Func<ILogger, Dictionary<string, object>, TRecord, TRecord> TransformInputToOutput { get; }
        #endregion

        #region Properties
        public FileInfo InputFile => InputFileName.IsEmpty() ? null : new FileInfo(InputFileName);

        public FileInfo OutputFile => OutputFileName.IsEmpty() ? null : new FileInfo(OutputFileName);

        public virtual List<TRecord> InputRecords { get; protected set; } = new List<TRecord>();

        public virtual List<TRecord> OutputRecords { get; protected set; } = new List<TRecord>();

        [Option('i', "input-file", Required = true, HelpText = "Input data file name for stage operation.")]
        public virtual string InputFileName { get; set; }

        [Option('f', "output-file", Required = true, HelpText = "Output data file name for stage operation.")]
        public virtual string OutputFileName { get; set; }

        public static SortedList<int, string> FeatureMap { get; } = new SortedList<int, string>();
        #endregion

        #region Methods
        protected virtual StageResult Transform(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null)
        {
            if (!ParallelExecution || InputRecords.Count < 100 || ((RecordLimitSize > 0) && (RecordLimitSize < 100)))
            {
                using (Operation transformOp = Begin("Transforming {0} records using sequential execution", InputRecords.Count))
                {
                    for (int i = 0; i < InputRecords.Count; i++)
                    {
                        OutputRecords.Add(TransformInputToOutput(L, WriterOptions, InputRecords[i]));
                        if ((i + 1) % 1000 == 0)
                        {
                            Info("Transformed range {0} to {1} of {2} records...", (i + 1) - 1000, i + 1, InputRecords.Count);
                        }
                        if ((RecordLimitSize > 0) && (i + 1 == RecordLimitSize))
                        {
                            Info("Stopping transformation at record limit {0}.", i + 1);
                            transformOp.Complete();
                            break;
                        }
                    }
                    transformOp.Complete();
                }
            }
            else
            {
                int limit = RecordLimitSize > 0 ? RecordLimitSize <= InputRecords.Count ? RecordLimitSize : InputRecords.Count : InputRecords.Count;
                using (Operation transformOp = Begin("Transforming {0} records using parallel execution", limit))
                {
                    ConcurrentDictionary<int, TRecord> concurrentOutputDictionary = new ConcurrentDictionary<int, TRecord>();
                    
                    Parallel.For(0, limit, (i, loop) =>
                    {
                        TRecord output = TransformInputToOutput(L, WriterOptions, InputRecords[i]);
                        concurrentOutputDictionary.TryAdd(i, output);
                        if ((i + 1) % 1000 == 0)
                        {
                            Info("Transformed range {0} to {1} of {2} records...", (i + 1) - 1000, i + 1, InputRecords.Count);
                        }
                    });
                    OutputRecords = concurrentOutputDictionary.Values.ToList();
                    transformOp.Complete();
                     
                }
            }
            Info("Transformed {0} records with maximum {1} features to {2}.", OutputRecords.Count, OutputRecords.Max(r => r.Features.Count), OutputFileName);
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
