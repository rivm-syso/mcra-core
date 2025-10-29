using CommandLine;

namespace ModuleDiagramCreator {
    [Verb("create", HelpText = "Create a MCRA module diagram for an action.")]
    public class CreateOptions {
        [Option('o', "output", Default = null, HelpText = "Output directory where the image file, and optionally the graph file, will be created. Default is the current working directory.")]
        public string OutputDir { get; set; } = Directory.GetCurrentDirectory();

        [Option('f', "diagramfilename", Default = _defaultDiagramFilename, HelpText = "Output directory where the image file, and optionally the graph file, will be created. Default is the current working directory.")]
        public string DiagramFilename { get; set; }

        [Option('a', "outputformat", Default = _defaultOutputFormat, HelpText = "The graphic and data format of the created image file, default SVG. For valid option, see https://graphviz.org/docs/outputs/")]
        public string OutputFormat { get; set; }

        [Option('d', "createdotfile", Default = true, HelpText = "Specify true (default) to also create the intermediate GraphViz dot file, specify false to not create the dot file.")]
        public bool OutputDotFile { get; set; }

        [Option('l', "layoutalgorithm", Default = _defaultLayoutAlgorithm, HelpText = "For Graphviz engine only. The layout engine to render the graph, see https://www.graphviz.org/pdf/dot.1.pdf")]
        public string LayoutAlgorithm { get; set; }

        public const string _defaultDiagramFilename = "modulardesign";
        public const string _defaultOutputFormat = "svg";
        public const string _defaultLayoutAlgorithm = "fdp";

        /// <summary>
        /// Node separation, space between nodes (in vertical direction).
        /// </summary>
        public double NodeSep { get; set; } = double.NaN;

        /// <summary>
        /// Space separation, space between ranks (in horizontal direction).
        /// </summary>
        public double RankSep { get; set; } = double.NaN;

        /// <summary>
        /// The default is 1, which means that action names are split into two lines.
        /// </summary>
        public int LineWrap { get; set; } = 1;

        public int Height { get; set; } = 10;

        public int Width { get; set; } = 15;
    }
}
