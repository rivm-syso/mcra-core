using System.Reflection;
using System.Xml.Serialization;
using ModuleDiagramCreator.Helpers;

namespace ModuleDiagramCreator.Resources {
    public class ModuleDiagramDefinitions {

        private static ModuleDiagramDefinitions _instance;

        public List<GraphDefinition> GraphDefinitions { get; set; } = new List<GraphDefinition>();

        public static ModuleDiagramDefinitions Instance {
            get { return _instance ??= LoadModuleDiagramsDefinitions(); }
        }

        private static ModuleDiagramDefinitions LoadModuleDiagramsDefinitions() {
            var assembly = Assembly.Load("ModuleDiagramCreator");
            using (var stream = assembly.GetManifestResourceStream("ModuleDiagramCreator.Resources.ModuleDiagramDefinitions.xml")) {
                var xs = new XmlSerializer(typeof(ModuleDiagramDefinitions));
                var result = (ModuleDiagramDefinitions)xs.Deserialize(stream);
                return result;
            }
        }
    }
}
