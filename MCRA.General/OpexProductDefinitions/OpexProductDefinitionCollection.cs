using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.OpexProductDefinitions {

    [Serializable()]
    [XmlRoot("OpexProductDefinitions")]
    public class OpexProductDefinitionCollection : Collection<OpexProductDefinition> {        
    }
}
