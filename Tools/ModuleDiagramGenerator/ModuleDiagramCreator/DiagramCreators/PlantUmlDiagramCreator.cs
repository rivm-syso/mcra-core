using System.Diagnostics;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace ModuleDiagramCreator.DiagramCreators {

    public class PlantUmlDiagramCreator : ModuleDiagramCreator , IModuleDiagramCreator {

        public void CreateToFile(
            CreateOptions options,
            string diagramFilename,
            string outputDir,
            ICollection<(ActionType actionType, ModuleType moduleType, List<string> inputs)> relationships,
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
                foreach (var relation in relationships) {
                    // Apply word wrap, line breaks when defined
                    var label = relation.actionType.GetDisplayName();
                    var space = " ";
                    var nrWrapLines = options.LineWrap;
                    while (nrWrapLines-- > 0) {
                        var pos = label.LastIndexOf(space);
                        if (pos == -1) {
                            break;
                        }
                        label = label.Remove(pos, space.Length).Insert(pos, "\n");
                    }
                    var color = GetColors(relation.moduleType);
                    color = color.Replace(":", "-");
                    if (relation.actionType.ToString() == indicateActionType) {
                        sb.WriteLine($"rectangle {relation.actionType} #{color} [ \n  Action: \n {label} \n]");
                    } else {
                        sb.WriteLine($"rectangle {relation.actionType} #{color} [ \n  {label} \n ]");
                    }
                }
                //Write all relations to file
                foreach (var relation in relationships) {
                    foreach (var input in relation.inputs) {
                        if (relation.moduleType != ModuleType.PrimaryEntityModule) {
                            sb.WriteLine($"{input} -->> {relation.actionType}");
                        } else {
                            sb.WriteLine($"{input} -->> {relation.actionType}");
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
