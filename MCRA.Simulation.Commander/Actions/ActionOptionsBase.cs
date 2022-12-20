using CommandLine;

namespace MCRA.Simulation.Commander {

    public enum RawDataManagerType {
        Binary,
        Csv
    };

    public class ActionOptionsBase {

        [Option('i', "interactive", Default = false, HelpText = "Set to run in interactive mode.")]
        public bool InteractiveMode { get; set; }

        [Option('s', "silent", Default = false, HelpText = "Set to run in silent mode.")]
        public bool SilentMode { get; set; }

        [Option("dbType", Default = RawDataManagerType.Csv, HelpText = "Database type.")]
        public RawDataManagerType RawDataManagerType { get; set; }

    }
}
