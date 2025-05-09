﻿using CommandLine;

namespace MCRA.Simulation.Commander.Actions.ConvertAction {

    [Verb("convert", HelpText = "Convert datafile to zipped csv or excel archive.")]
    public class ConvertActionOptions : ActionOptionsBase {

        [Value(0, MetaName = "Task input file", HelpText = "Input file containing the input data file.", Required = true)]
        public string InputPath { get; set; }

        [Option("keeptempfiles", Default = false, HelpText = "Keep temp files.")]
        public bool KeepTempFiles { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output file.")]
        public string OutputFileName { get; set; }

        [Option("overwrite", Default = false, HelpText = "Overwrite existing output.")]
        public bool OverwriteOutput { get; set; }

        [Option("recoding-file", Default = null, HelpText = "Recoding configuration.")]
        public string EntityRecodingFileName { get; set; }

        [Option('t', "filetype", Default = "csv", HelpText = "Output file type (csv or excel)")]
        public string FileType { get; set; } = "csv";
    }
}
