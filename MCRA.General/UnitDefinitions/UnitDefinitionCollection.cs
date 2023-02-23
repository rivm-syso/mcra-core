using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General {
    [Serializable()]
    [XmlRoot("UnitDefinitions")]
    public sealed class UnitDefinitionCollection : Collection<UnitDefinition> {
    }
}
