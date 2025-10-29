using System.Diagnostics;
using System.Text;
using GraphVizNet;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.Utils.ExtensionMethods;
using ModuleDiagramCreator.Helpers;
using ModuleDiagramCreator.Resources;

namespace ModuleDiagramCreator.DiagramCreators {

    public class GraphvizDiagramCreator : ModuleDiagramCreator, IModuleDiagramCreator {

        private readonly string _colorUndefined = "ghostwhite";
        private readonly string _colorEdge = "black";
        private readonly int _fontSize = 26;
        private readonly string _fontName = "Calibri";
        private readonly int _fontWeight = 700;
        private readonly int _scalingFactor = 4; // This is an arbitrary factor to reduce to total size to acceptable proportions, should be smarted and based on e.g., pixels
        private readonly double _nodeWidth = 3.0;
        private readonly double _nodeHeight = 1.5;
        private readonly double _nodeHeightFullGraph = 1.3;
        private readonly string _extensionDotFile = "dot";
        private readonly string _documentationBaseUrl = $"https://mcra.rivm.nl/documentation/{ThisAssembly.Git.BaseVersion.Major}.{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}/modules/";

        /// <summary>
        /// The location of the GraphViz binaries directory.
        /// </summary>
        public string GraphVizBinariesDirectory { get; set; } = "graphviz";

        public void CreateToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            ICollection<(ActionType, ModuleType, List<string>)> relationships,
            string indicateActionType = null
        ) {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var dotString = new StringBuilder();
            CreateHeader(options, _nodeHeight, ref dotString);
            AddModules(options, relationships, indicateActionType, ref dotString);
            AddRelationships(relationships, ref dotString);
            CreateFooter(ref dotString);
            var imageFile = WriteToFile(
                options,
                diagramFilename,
                outputDir,
                dotString.ToString()
            );
            ApplyFontWeight(imageFile);
        }

        public int CreateToFile(
            CreateOptions options
        ) {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var dotString = new StringBuilder();
            var relationships = new Dictionary<string, List<string>>();

            CreateHeader(options, _nodeHeightFullGraph, ref dotString);
            AddModules(ref dotString, ref relationships);
            AddRelationships(ref dotString, relationships);
            AddLegend(ref dotString);
            CreateFooter(ref dotString);

            var imageFile = WriteToFile(
                options,
                options.DiagramFilename,
                options.OutputDir,
                dotString.ToString()
            );
            ApplyFontWeight(imageFile);

            return 0;
        }

        private void CreateHeader(CreateOptions options, double nodeHeight, ref StringBuilder sb) {
            sb.AppendLine("digraph {");
            sb.AppendLine("splines = ortho;");
            if (options.LayoutAlgorithm != "fdp") {
                sb.AppendLine("overlap = false;");
            }
            sb.AppendLine($"edge [fontname=\"{_fontName}\", color=\"{_colorEdge}\"];");
            if (double.IsNaN(options.NodeSep) || double.IsNaN(options.RankSep)) {
                sb.AppendLine("graph [rankdir=\"LR\", " +
                    $"size = \"{options.Height},{options.Width}\", " +
                    $"orientation = \"90 \"];"
                );
            } else {
                sb.AppendLine("graph [rankdir=\"LR\", " +
                    $"size = \"{options.Height},{options.Width}\", " +
                    $"orientation = \"90 \", " +
                    $"nodesep = \"{options.NodeSep} equally\", " +
                    $"ranksep = \"{options.RankSep} equally\"];"
                );
            }
            sb.AppendLine($"node [fontname=\"{_fontName}\", " +
                $"fontsize=\"{_fontSize}\", " +
                $"shape=\"box\", " +
                $"style=\"filled\", " +
                $"gradientangle= \"90\", " +
                $"width= \"{_nodeWidth}\"," +
                $"height=\"{nodeHeight}\"] "
            );
        }

        private void AddModules(
            CreateOptions options,
            ICollection<(ActionType actionType, ModuleType moduleType, List<string>)> relationships,
            string indicateActionType,
            ref StringBuilder sb
        ) {
            foreach (var relation in relationships) {
                AddModule(
                    options,
                    relation.actionType,
                    relation.moduleType,
                    indicateActionType,
                    ref sb
                );
            }
        }

        private void AddModules(ref StringBuilder sb, ref Dictionary<string, List<string>> relationships) {
            var modules = McraModuleDefinitions.Instance.ModuleDefinitions;
            var graphDefinitions = ModuleDiagramDefinitions.Instance.GraphDefinitions;

            foreach (var module in modules) {
                var graphDefinition = graphDefinitions.FirstOrDefault(d => d.ActionType == module.Key);
                //Debug.Assert(graphDefinition != null, $"No graph definition found for module '{module.Key}'. Please add a graph definition for '{module.Key}' to file {ModuleDiagramDefinitions._moduleDiagramDefinitionFile}.");
                if (graphDefinition != null) {
                    AddModule(ref sb, module.Value, graphDefinition, ref relationships);
                }
            }
        }

        private void AddModule(
            CreateOptions options,
            ActionType actionType,
            ModuleType moduleType,
            string indicateActionType,
            ref StringBuilder sb
        ) {
            // Apply word wrap, line breaks when defined
            var label = actionType.GetDisplayName();
            var space = " ";
            var nrWrapLines = options.LineWrap;
            while (nrWrapLines-- > 0) {
                var pos = label.LastIndexOf(space);
                if (pos == -1) {
                    break;
                }
                label = label.Remove(pos, space.Length).Insert(pos, "\n");
            }
            var color = GetColors(moduleType);
            label = actionType.ToString() == indicateActionType ? $"Action: \n {label}" : label;
            sb.AppendLine($"\"{actionType}\" [label=\"{label}\", " +
                    $"gradientangle=\"90\", " +
                    $"fillcolor=\"{color}\", " +
                    $"href=\"{CreateUrl(actionType)}\", target =\"_blank\"]");
        }

        private void AddModule(
           ref StringBuilder sb,
           ModuleDefinition moduleDefinition,
           GraphDefinition graphDefinition,
           ref Dictionary<string, List<string>> relationships
       ) {
            var fillcolor = _colorUndefined;
            if (moduleDefinition.ModuleType == ModuleType.DataModule) {
                fillcolor = DataColor;
            } else if (moduleDefinition.ModuleType == ModuleType.CalculatorModule) {
                fillcolor = string.IsNullOrEmpty(moduleDefinition.TableGroup) ? CalculatorColor : DataAndCalculatorColor;
            } else if (moduleDefinition.ModuleType == ModuleType.PrimaryEntityModule) {
                fillcolor = EntityColor;
            }

            // Apply word wrap, line breaks when defined
            var label = moduleDefinition.Name;
            var space = " ";
            var nrWrapLines = graphDefinition.Linebreaks;
            while (nrWrapLines-- > 0) {
                var pos = label.LastIndexOf(space);
                if (pos == -1) {
                    break;
                }
                label = label.Remove(pos, space.Length).Insert(pos, "\n");
            }

            var xPos = graphDefinition.X / _scalingFactor;
            var yPos = graphDefinition.Y / _scalingFactor;
            sb.AppendLine($"\"{moduleDefinition.ActionType}\" [label=\"{label}\", " +
                                                            $"gradientangle=\"90\", " +
                                                            $"fillcolor=\"{fillcolor}\", " +
                                                            $"pos=\"{xPos},{yPos}!\", " +
                                                            $"URL=\"{CreateUrl(moduleDefinition.ActionType)}\"]");

            // Add relations for module inputs
            foreach (var calculationInput in moduleDefinition.CalculatorInputs) {
                if (!relationships.ContainsKey(calculationInput.ToString())) {
                    relationships[calculationInput.ToString()] = [moduleDefinition.ActionType.ToString()];
                } else {
                    relationships[calculationInput.ToString()].Add(moduleDefinition.ActionType.ToString());
                }
            }

            // Add relations for primary entities
            foreach (var entity in moduleDefinition.Entities) {
                if (!relationships.ContainsKey(entity)) {
                    relationships[entity] = [moduleDefinition.ActionType.ToString()];
                } else {
                    relationships[entity].Add(moduleDefinition.ActionType.ToString());
                }
            }
        }

        private void AddRelationships(
           ICollection<(ActionType actionType, ModuleType moduleType, List<string> input)> relationships,
           ref StringBuilder sb
        ) {
            foreach (var relation in relationships) {
                var module = relation.actionType;
                foreach (var input in relation.input) {
                    sb.AppendLine($"\"{input}\" -> \"{module}\" [edgetooltip=\"From: {input}, To: {module}\"];");
                }
            }
        }

        private void AddRelationships(ref StringBuilder sb, Dictionary<string, List<string>> relationships) {
            foreach (var relationship in relationships) {
                var module = relationship.Key;
                foreach (var input in relationship.Value) {
                    sb.AppendLine($"\"{module}\" -> \"{input}\" [edgetooltip=\"From: {module}, To: {input}\"];");
                }
            }
        }

        private void AddLegend(ref StringBuilder sb) {
            sb.AppendLine($"\"PrimaryEntities\" [label=\"Primary entities\", " + $"gradientangle=\"90\", " +
                            $"fillcolor=\"{EntityColor}\", " + $"pos=\"0,-3!\"]");
            sb.AppendLine($"\"Data\" [label=\"Data\", " + $"gradientangle=\"90\", " +
                            $"fillcolor=\"{DataColor}\", " + $"pos=\"4,-3!\"]");
            sb.AppendLine($"\"Calculator\" [label=\"Calculator\", " + $"gradientangle=\"90\", " +
                            $"fillcolor=\"{CalculatorColor}\", " + $"pos=\"8,-3!\"]");
            sb.AppendLine($"\"DataCalculator\" [label=\"Data - Calculator\", " + $"gradientangle=\"90\", " +
                            $"fillcolor=\"{DataAndCalculatorColor}\", " + $"pos=\"12,-3!\"]");
        }

        private string WriteToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            string dotString
        ) {
            // Write the .dot file
            if (options.OutputDotFile) {
                var dotFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(diagramFilename));
                dotFile = Path.ChangeExtension(dotFile, _extensionDotFile);
                File.WriteAllText(dotFile, dotString);
            }
            // Write module diagram file
            var imageFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(diagramFilename));
            imageFile = Path.ChangeExtension(imageFile, options.OutputFormat);
            var graphViz = new GraphViz();
            graphViz.Config.GraphVizBinariesDirectory = GraphVizBinariesDirectory;
            graphViz.LayoutAndRender(null, dotString, imageFile, options.LayoutAlgorithm, options.OutputFormat);
            return imageFile;
        }

        private void ApplyFontWeight(string imageFile) {
            var text = File.ReadAllText(imageFile);
            text = text.Replace($"font-family=\"{_fontName}\"", $"font-family=\"{_fontName}\" font-weight=\"{_fontWeight}\"");
            File.WriteAllText(imageFile, text);
        }

        private void CreateFooter(ref StringBuilder sb) {
            sb.AppendLine("}");
        }

        private string CreateUrl(ActionType actionType) {
            var actionClass = McraModuleDefinitions.Instance.GetActionClass(actionType);
            var moduleGroupParts = actionClass.GetDisplayName().ToLower().Split(" ").ToList();
            moduleGroupParts.Add("modules");
            var moduleGroup = string.Join("-", moduleGroupParts);
            var name = actionType.GetDisplayName();
            name = name.Replace(' ', '-');
            var url = _documentationBaseUrl + moduleGroup + "/" + name.ToLower() + "/index.html";
            return url;
        }
    }
}
