using System.Diagnostics;
using System.Text;
using GraphVizNet;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.Utils.ExtensionMethods;
using ModuleDefinition = MCRA.General.ModuleDefinitions.ModuleDefinition;

namespace ModuleDiagramCreator {
    internal class DiagramCreator {
        private readonly string _colorPrimaryEntity = "lavender";
        private readonly string _colorData = "yellowgreen";
        private readonly string _colorCalculator = "gold";
        private readonly string _colorDataCalculator = "gold:yellowgreen";
        private readonly string _colorUndefined = "ghostwhite";
        private readonly string _colorEdge = "black";
        private readonly int _fontSize = 26;
        private readonly string _fontName = "Calibri";
        private readonly int _fontWeight = 700;
        private readonly int _scalingFactor = 4;                     // This is an arbitrary factor to reduce to total size to acceptable proportions, should be smarted and based on e.g., pixels
        private readonly double _nodeWidth = 3.0;
        private readonly double _nodeHeight = 1.3;

        private readonly string _extensionDotFile = "dot";
        private readonly string _layoutAlgorithm = "fdp";            // see https://www.graphviz.org/pdf/dot.1.pdf
        private readonly string _documentationBaseUrl = $"https://mcra.rivm.nl/documentation/{ThisAssembly.Git.BaseVersion.Major}.{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}/modules/";

        public int Create(CreateOptions options) {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            StringBuilder dotString = new StringBuilder();
            var relationships = new Dictionary<string, List<string>>();

            CreateHeader(ref dotString);
            AddModules(ref dotString, ref relationships);
            AddRelationships(ref dotString, relationships);
            AddLegend(ref dotString);
            CreateFooter(ref dotString);

            var imageFile = WriteToFile(options, dotString.ToString());
            ApplyFontWeight(imageFile);

            return 0;
        }

        private void CreateHeader(ref StringBuilder sb) {
            sb.AppendLine("digraph {");
            sb.AppendLine("splines=ortho;");
            sb.AppendLine($"edge [fontname=\"{_fontName}\", color=\"{_colorEdge}\"];");
            sb.AppendLine("graph [rankdir=\"LR\"];");
            sb.AppendLine($"node [fontname=\"{_fontName}\", fontsize=\"{_fontSize}\", shape=\"box\", style=\"filled\", gradientangle=90, width={_nodeWidth}, height={_nodeHeight}];");
        }

        private void AddModules(ref StringBuilder sb, ref Dictionary<string, List<string>> relationships) {
            var modules = McraModuleDefinitions.Instance.ModuleDefinitions;
            var graphDefinitions = ModuleDiagramDefinitions.Instance.GraphDefinitions;

            foreach (var module in modules) {
                var graphDefinition = graphDefinitions.FirstOrDefault(d => d.ActionType == module.Key);
                Debug.Assert(graphDefinition != null, $"No graph definition found for module '{module.Key}'. Please add a graph definition for '{module.Key}' to file {ModuleDiagramDefinitions._moduleDiagramDefinitionFile}.");
                if (graphDefinition != null) {
                    AddModule(ref sb, module.Value, graphDefinition, ref relationships);
                }
            }
        }

        private void AddRelationships(ref StringBuilder sb, Dictionary<string, List<string>> relationships) {
            foreach (var relationship in relationships) {
                var module = relationship.Key;
                foreach (var inputActionType in relationship.Value) {
                    sb.AppendLine($"\"{module}\" -> \"{inputActionType}\" [edgetooltip=\"From: {module}, To: {inputActionType}\"];");
                }
            }
        }

        private void AddLegend(ref StringBuilder sb) {
            sb.AppendLine($"\"PrimaryEntities\" [label=\"Primary entities\", " + $"gradientangle=\"270\", " +
                            $"fillcolor=\"{_colorPrimaryEntity}\", " + $"pos=\"0,-3!\"]");
            sb.AppendLine($"\"Data\" [label=\"Data\", " + $"gradientangle=\"270\", " +
                            $"fillcolor=\"{_colorData}\", " + $"pos=\"4,-3!\"]");
            sb.AppendLine($"\"Calculator\" [label=\"Calculator\", " + $"gradientangle=\"270\", " +
                            $"fillcolor=\"{_colorCalculator}\", " + $"pos=\"8,-3!\"]");
            sb.AppendLine($"\"DataCalculator\" [label=\"Data - Calculator\", " + $"gradientangle=\"270\", " +
                            $"fillcolor=\"{_colorDataCalculator}\", " + $"pos=\"12,-3!\"]");
        }

        private void AddModule(ref StringBuilder sb, ModuleDefinition moduleDefinition, GraphDefinition graphDefinition, ref Dictionary<string, List<string>> relationships) {
            //if (moduleDefinition.Graph == null) {
            //    Trace.TraceWarning($"Module '{moduleDefinition.Name}' not added to MCRA module diagram. Please add a Graph element to the ModuleDefinition XML file.");
            //    return;
            //}

            string fillcolor = _colorUndefined;
            if (moduleDefinition.ModuleType == ModuleType.DataModule) {
                fillcolor = _colorData;
            } else if (moduleDefinition.ModuleType == ModuleType.CalculatorModule) {
                fillcolor = string.IsNullOrEmpty(moduleDefinition.TableGroup) ? _colorCalculator : _colorDataCalculator;
            } else if (moduleDefinition.ModuleType == ModuleType.PrimaryEntityModule) {
                fillcolor = _colorPrimaryEntity;
            }

            // Apply word wrap, line breaks when defined
            var label = moduleDefinition.Name;
            var space = " ";
            int nrWrapLines = graphDefinition.Linebreaks;
            while (nrWrapLines-- > 0) {
                int pos = label.LastIndexOf(space);
                if (pos == -1) {
                    break;
                }
                label = label.Remove(pos, space.Length).Insert(pos, "\n");
            }

            var xPos = graphDefinition.X / _scalingFactor;
            var yPos = graphDefinition.Y / _scalingFactor;
            sb.AppendLine($"\"{moduleDefinition.ActionType}\" [label=\"{label}\", " +
                                                            $"gradientangle=\"270\", " +
                                                            $"fillcolor=\"{fillcolor}\", " +
                                                            $"pos=\"{xPos},{yPos}!\", " +
                                                            $"URL=\"{CreateUrl(moduleDefinition)}\"]");

            // Add relations for module inputs
            foreach (var calculationInput in moduleDefinition.CalculatorInputs) {
                if (!relationships.ContainsKey(calculationInput.ToString())) {
                    relationships[calculationInput.ToString()] = new List<string> { moduleDefinition.ActionType.ToString() };
                } else {
                    relationships[calculationInput.ToString()].Add(moduleDefinition.ActionType.ToString());
                }
            }

            // Add relations for primary entities
            foreach (var entity in moduleDefinition.Entities) {
                if (!relationships.ContainsKey(entity)) {
                    relationships[entity] = new List<string> { moduleDefinition.ActionType.ToString() };
                } else {
                    relationships[entity].Add(moduleDefinition.ActionType.ToString());
                }
            }
        }

        private string WriteToFile(CreateOptions options, string dotString) {
            // Write the .dot file
            if (options.OutputDotFile) {
                var dotFile = Path.Combine(options.OutputDir, Path.GetFileNameWithoutExtension(options.DiagramFilename));
                dotFile = Path.ChangeExtension(dotFile, _extensionDotFile);
                File.WriteAllText(dotFile, dotString);
            }

            // Write module diagram file
            var imageFile = Path.Combine(options.OutputDir, Path.GetFileNameWithoutExtension(options.DiagramFilename));
            imageFile = Path.ChangeExtension(imageFile, options.OutputFormat);
            var graphViz = new GraphViz();
            graphViz.LayoutAndRender(null, dotString, imageFile, _layoutAlgorithm, options.OutputFormat);

            return imageFile;
        }

        private void ApplyFontWeight(string imageFile) {
            string text = File.ReadAllText(imageFile);
            text = text.Replace($"font-family=\"{_fontName}\"", $"font-family=\"{_fontName}\" font-weight=\"{_fontWeight}\"");
            File.WriteAllText(imageFile, text);
        }

        private void CreateFooter(ref StringBuilder sb) {
            sb.AppendLine("}");
        }

        private string CreateUrl(ModuleDefinition moduleDefinition) {
            var actionClass = McraModuleDefinitions.Instance.GetActionClass(moduleDefinition.ActionType);
            var moduleGroupParts = actionClass.GetDisplayName().ToLower().Split(" ").ToList();
            moduleGroupParts.Add("modules");
            string moduleGroup = string.Join("-", moduleGroupParts);

            string name = moduleDefinition.Name;
            name = name.Replace(' ', '-');

            string url = _documentationBaseUrl + moduleGroup + "/" + name.ToLower() + "/" + "index.html";

            return url;
        }
    }
}
