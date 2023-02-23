using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions {

    [Serializable()]
    [XmlRoot("ModuleDefinitions")]
    public sealed class ModuleGroupDefinitionCollection : Collection<ModuleGroupDefinition> {

    }
}
