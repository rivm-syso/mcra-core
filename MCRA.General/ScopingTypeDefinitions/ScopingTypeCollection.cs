using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.ScopingTypeDefinitions {

    [Serializable()]
    [XmlRoot("ScopingTypeDefinitions")]
    public class ScopingTypeCollection : Collection<ScopingTypeDefinition> {
    }
}
