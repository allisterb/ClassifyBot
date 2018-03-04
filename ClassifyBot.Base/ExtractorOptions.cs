using System;
using System.Collections.Generic;
using System.Text;

namespace ClassifyBot
{
    public abstract class ExtractorOptions : CommandLineOptions
    {
        [Option('f', "output-file", Required = true, HelpText = "Output data file name for dataset. A file with .json or .json.gz extension will be created with this name.")]
        public string OutputFile { get; set; }

        [Option('o', "overwrite", Required = false, Default = false, HelpText = "Ovewrite existing output data file if it exists.")]
        public bool OverwriteOutputFile { get; set; }

        [Option('a', "append", Required = false, Default = false, HelpText = "Append extracted data to existing output file if it exists.")]
        public bool AppendToOutputFile { get; set; }

        [Option('c', "compress", Required = false, Default = false, HelpText = "Output file will be compressed with GZIP.")]
        public bool CompressOutputFile { get; set; }

        [Option('b', "batch", Required = false, HelpText = "Batch the number of records extracted.", Default = 0)]
        public int RecordBatchSize { get; set; }

        [Option('l', "records", Required = false, HelpText = "Limit the number of records extracted.", Default = 0)]
        public int RecordLimit { get; set; }

    }
}
