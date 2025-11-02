using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GraphVizNet;
using MCRA.General;
using MCRA.General.Sbml;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.Objects;
using ModuleDiagramCreator.DiagramCreators;

namespace MCRA.Simulation.OutputGeneration {

    public class PbkDiagramOptions {
        public int Dpi { get; set; } = 100;
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 400;
        public int Scale { get; set; } = 4;
        public bool DrawEdgeLabels { get; set; } = false;
        public bool OutputDotFile { get; set; } = true;
        public string LayoutAlgorithm { get; set; } = "dot";
        public string OutputFormat { get; set; } = "svg";
        public string EdgeColor { get; set; } = "black";
        public string Splines { get; set; } = "curved";
        public int FontSize { get; set; } = 10;
        public string FontName { get; set; } = "Calibri";
        public int FontWeight { get; set; } = 700;
    }

    public sealed class PbkModelDiagramCreator : IReportChartCreator {

        private readonly PbkModelDefinitionSummarySection _section;

        public int Width { get; set; }
        public int Height { get; set; }

        public string ChartId {
            get {
                var chartId = "de85afff-2fbd-497b-a0e8-c03773cb8f9e";
                return StringExtensions.CreateFingerprint(_section.SectionId + chartId);
            }
        }

        public string Title => $"Diagram of PBK model {_section.ModelName}.";

        public PbkModelDiagramCreator(
            PbkModelDefinitionSummarySection section
        ) {
            Width = 800;
            Height = 450;
            _section = section;
        }

        public void CreateToSvg(string fileName) {
            Create(fileName, "svg");
        }

        public void CreateToPng(string fileName) {
            Create(fileName, "png");
        }

        public string ToSvgString(int width, int height) {
            var imgBytes = Create(null, "svg");
            var result = Encoding.Default.GetString(imgBytes);
            var curWidth = double.Parse(Regex.Match(result, @"width=""(\d+(?:\.\d+)?)pt""").Groups[1].Value);
            // If svg width larger than specified width, then rescale
            if (curWidth > width) {
                var pattern = @"width\s*=\s*""[^""]*""\s*height\s*=\s*""[^""]*""";
                var replacement = $"width=\"{Width}px\" height=\"{Height}px\"";
                result = Regex.Replace(result, pattern, replacement, RegexOptions.IgnoreCase);
            }
            return result;
        }

        public void WritePngToStream(Stream stream) {
            throw new NotImplementedException();
        }

        private byte[] Create(
            string fileName,
            string outputFormat
        ) {
            var diagramCreator = new GraphvizDiagramCreator {
                GraphVizBinariesDirectory = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "graphviz"
                )
            };
            var options = new PbkDiagramOptions() {
                OutputFormat = outputFormat,
                Dpi = 100,
                Scale = outputFormat == "png" ? 4 : 1,
                Height = Height,
                Width = Width,
            };
            var dotString = createDot(_section.SbmlModel, options);

            if (options.OutputDotFile && !string.IsNullOrEmpty(fileName)) {
                // Write the .dot file
                var outputDir = Path.GetDirectoryName(fileName);
                var dotFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(fileName));
                dotFile = Path.ChangeExtension(dotFile, ".dot");
                File.WriteAllText(dotFile, dotString);
            }

            // Write module diagram file
            var graphViz = new GraphViz();
            var graphVizBinariesDirectory = Path
                .Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "graphviz");
            graphViz.Config.GraphVizBinariesDirectory = graphVizBinariesDirectory;
            var result = graphViz.LayoutAndRender(
                null,
                dotString,
                fileName,
                options.LayoutAlgorithm,
                options.OutputFormat
            );
            return result;
        }

        public string createDot(
            SbmlModel model,
            PbkDiagramOptions options
        ) {
            var sb = new StringBuilder();
            sb.AppendLine($"digraph {model.Id} {{");

            if (options.LayoutAlgorithm != "fdp") {
                sb.AppendLine("  overlap = false;");
            }
            sb.AppendLine(
                "  graph [\n" +
                "    rankdir=TB,\n" +
                $"   fontname={options.FontName},\n" +
                $"   fontsize={options.FontSize},\n" +
                "  ];"
            );
            sb.AppendLine(
                $"  node [\n" +
                $"    fontname=\"{options.FontName}\",\n" +
                $"    fontsize=\"{options.FontSize}\",\n" +
                $"    shape=\"box\",\n" +
                $"    style=\"filled\",\n" +
                $"  ];"
            );
            sb.AppendLine(
                $"  edge [\n" +
                $"    fontname=\"{options.FontName}\",\n" +
                $"    fontsize={options.FontSize},\n" +
                $"    color=\"{options.EdgeColor}\"\n" +
                $"  ];"
            );

            var speciesByCompartment = model.Species.ToLookup(r => r.Compartment);
            foreach (var compartment in model.Compartments) {
                var matrix = compartment.GetBiologicalMatrix();
                var compartmentLabel = compartment.Id;
                sb.AppendLine($"  subgraph cluster_{compartment.Id} {{");
                sb.AppendLine(
                    "    graph [\n" +
                    "      color=black,\n" +
                    "      fillcolor=lightblue,\n" +
                    $"      label=\"{compartmentLabel}\",\n" +
                    "      shape=box,\n" +
                    "      style=\"filled,rounded\"\n" +
                    "    ];"
                );
                if (speciesByCompartment.Contains(compartment.Id)) {
                    var species = speciesByCompartment[compartment.Id];
                    foreach (var item in species) {
                        var speciesLabel = item.Id;
                        sb.AppendLine(
                            $"    {item.Id} [\n" +
                            "      color=black,\n" +
                            "      fillcolor=gold,\n" +
                            $"      label=\"{speciesLabel}\",\n" +
                            "      shape=box,\n" +
                            "      style=\"filled,rounded\"\n" +
                            "    ];"
                        );
                    }
                }
                sb.AppendLine("  }");
            }

            foreach (var reaction in model.Reactions) {
                foreach (var reactant in reaction.Reactants) {
                    foreach (var product in reaction.Products) {
                        sb.Append($"  \"{reactant}\" -> \"{product}\"");
                        var attributes = new List<(string attr, string val)>();
                        if (options.DrawEdgeLabels) {
                            attributes.Add(("label", reaction.Id));
                        }
                        if (attributes.Count > 0) {
                            sb.Append(" [\n");
                            foreach (var attr in attributes) {
                                sb.AppendLine($"    {attr.attr} = {attr.val}");
                            }
                            sb.Append(" ];");
                        }
                        sb.Append("\n");
                    }
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
