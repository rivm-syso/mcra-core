using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General {

    [Serializable]
    [XmlRoot("PbkModelSpecifications")]
    public class EmbdeddedPbkModelSpecificationCollection : Collection<EmbeddedPbkModelSpecification> {
    }
}
