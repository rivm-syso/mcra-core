using CommandLine;
using MCRA.General.FileDefinitions;

namespace MCRA.Simulation.Commander.Actions.CreateAction {

    [Verb("create", HelpText = "Create a new MCRA action.")]
    public class CreateActionOptions : ActionOptionsBase {

        [Value(0, MetaName = "Name of action", Default = FileDefinitions.DefaultActionName, HelpText = "A custom name for the new action.", Required = false)]
        public string Name { get; set; }

        [Option('t', "type", Default = "Foods", HelpText = $"The type of the new action. For a list of supported action types, use the -u switch, i.e. run the command \'mcra.exe create -u\'.")]
        public string ActionType { get; set; }

        [Option('u', "supported", Default = false, HelpText = $"Prints a list of supported action types.")]
        public bool SupportedTypes { get; set; }

        [Option('o', "output", Default = null, HelpText = "Output folder where the new action files will be created. Default is the current working folder.")]
        public string OutputPath { get; set; } = Directory.GetCurrentDirectory();
    }
}
