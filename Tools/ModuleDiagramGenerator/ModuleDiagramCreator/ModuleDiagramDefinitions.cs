using System.Xml.Serialization;
using MCRA.Utils.Xml;

namespace ModuleDiagramCreator {
    public class ModuleDiagramDefinitions {

        private static ModuleDiagramDefinitions _instance;

        [XmlIgnore]
        public static string _moduleDiagramDefinitionFile = "ModuleDiagramDefinitions.xml";

        public List<GraphDefinition> GraphDefinitions { get; set; } = new List<GraphDefinition>();

        public static ModuleDiagramDefinitions Instance {
            get { return _instance ??= LoadModuleDiagramsDefinitions(); }
        }

        private static ModuleDiagramDefinitions LoadModuleDiagramsDefinitions() {
            return XmlSerialization.FromXmlFile<ModuleDiagramDefinitions>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _moduleDiagramDefinitionFile));
        }
    }
}
