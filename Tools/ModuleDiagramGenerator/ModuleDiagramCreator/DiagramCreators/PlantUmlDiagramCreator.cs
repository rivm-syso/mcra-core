using System.Diagnostics;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace ModuleDiagramCreator.DiagramCreators {

    public class PlantUmlDiagramCreator : ModuleDiagramCreator , IModuleDiagramCreator {

        public void CreateToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            Dictionary<(ActionType, ModuleType), List<string>> relationships,
            string indicateActionType = null
        ) {

            var designName = $"PlantUml {diagramFilename}";
            var umlFile = Path.Combine(outputDir, $"{diagramFilename}.puml");
            using (StreamWriter sb = new StreamWriter(umlFile)) {
                //Write header
                sb.WriteLine($"@startuml {designName}");
                sb.WriteLine($"skinparam linetype ortho");
                sb.WriteLine($"skinparam nodesep {options.NodeSep}");
                sb.WriteLine($"skinparam ranksep {options.RankSep}");
                sb.WriteLine($"skinparam SequenceArrowThickness .5");
                sb.WriteLine($"skinparam padding 0.5");

                //Write all retangles to file
                foreach (var definition in relationships) {
                    // Apply word wrap, line breaks when defined
                    var label = definition.Key.Item1.GetDisplayName();
                    var space = " ";
                    var nrWrapLines = options.LineWrap;
                    while (nrWrapLines-- > 0) {
                        var pos = label.LastIndexOf(space);
                        if (pos == -1) {
                            break;
                        }
                        label = label.Remove(pos, space.Length).Insert(pos, "\n");
                    }
                    var color = GetColors(definition.Key.Item2);
                    color = color.Replace(":", "-");
                    if (definition.Key.Item1.ToString() == indicateActionType) {
                        sb.WriteLine($"rectangle {definition.Key.Item1} #{color} [ \n  Action: \n {label} \n]");
                    } else {
                        sb.WriteLine($"rectangle {definition.Key.Item1} #{color} [ \n  {label} \n ]");
                    }
                }
                //Write all relations to file
                foreach (var relation in relationships) {
                    foreach (var item in relation.Value) {
                        if (relation.Key.Item2 != ModuleType.PrimaryEntityModule) {
                            sb.WriteLine($"{item} -->> {relation.Key.Item1}");
                        } else {
                            sb.WriteLine($"{item} -->> {relation.Key.Item1}");
                        }
                    }
                }
                sb.WriteLine($"@enduml {designName}");
            }
            exportToSvg(
                diagramFilename,
                outputDir
            );
        }

        public int CreateToFile(CreateOptions options) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Re-install Java component located in outputDir
        /// </summary>
        /// <param name="options"></param>
        private void exportToSvg(
            string diagramFilename,
            string outputDir
        ) {
            var jarFile = Path.Combine(outputDir, $"plantuml-1.2024.7.jar");
            var svgFile = Path.Combine(outputDir, $"{diagramFilename}.puml");
            var cmdPuml = $"java -jar {jarFile} {svgFile} -tsvg";

            Process pro = new Process();
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.CreateNoWindow = true;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.UseShellExecute = false;
            pro.Start();
            pro.StandardInput.WriteLine(cmdPuml);
            pro.StandardInput.Flush();
            pro.StandardInput.Close();
            pro.WaitForExit();
            Console.WriteLine(pro.StandardOutput.ReadToEnd());
            Process.Start("CMD.exe", cmdPuml);
        }
    }
}
