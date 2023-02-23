using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions {
    public sealed class ModuleGroupDefinition {

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ActionClass ActionClass { get; set; }

        [XmlArrayItem("Module", typeof(ModuleDefinition))]
        public List<ModuleDefinition> Modules { get; set; }
    }
}
