using CommandLine;

namespace MCRA.Simulation.Commander.Actions.RunAction {

    [Verb("run", HelpText = "Run MCRA simulation task.")]
    public class RunActionOptions : ActionOptionsBase {

        [Value(0, MetaName = "Task input folder", HelpText = "Input folder containing the simulation task to be processed. If not specified, it will use the current directory.")]
        public string InputPath { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output folder.")]
        public string OutputPath { get; set; }

        [Option("overwrite", Default = false, HelpText = "Overwrite existing output.")]
        public bool OverwriteOutput { get; set; }

        [Option("skipreport", Default = false, HelpText = "Don't render report.")]
        public bool SkipReport { get; set; }

        [Option("skiptables", Default = false, HelpText = "Don't generate tables.")]
        public bool SkipTables { get; set; }

        [Option("skipcharts", Default = false, HelpText = "Don't generate charts.")]
        public bool SkipCharts { get; set; }

        [Option("keeptempfiles", Default = false, HelpText = "Keep temp files.")]
        public bool KeepTempFiles { get; set; }

        [Option('r', "randomseed", Default = null, HelpText = "Use this value as the Monte Carlo random seed for the project.")]
        public int? RandomSeed { get; set; }

    }
}
