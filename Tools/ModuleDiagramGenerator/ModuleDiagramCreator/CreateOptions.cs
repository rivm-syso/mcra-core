using CommandLine;

namespace ModuleDiagramCreator
{
    [Verb("create", HelpText = "Create a new MCRA module diagram.")]
    public class CreateOptions {

        [Option('o', "output", Default = null, HelpText = "Output directory where the image file, and optionally the graph file, will be created. Default is the current working directory.")]
        public string OutputDir { get; set; } = Directory.GetCurrentDirectory();

        [Option('f', "diagramfilename", Default = _defaultDiagramFilename, HelpText = "Output directory where the image file, and optionally the graph file, will be created. Default is the current working directory.")]
        public string DiagramFilename { get; set; }

        [Option('a', "outputformat", Default = _defaultOutputFormat, HelpText = "The graphic and data format of the created image file, default SVG. For valid option, see https://graphviz.org/docs/outputs/")]
        public string OutputFormat { get; set; }

        [Option('d', "createdotfile", Default = true, HelpText = "Specify true (default) to also create the intermediate GraphViz dot file, specify false to not create the dot file.")]
        public bool OutputDotFile { get; set; }

        public const string _defaultDiagramFilename = "modulardesign";
        public const string _defaultOutputFormat = "svg";
    }
}
