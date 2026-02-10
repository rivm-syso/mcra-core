using System.Collections.ObjectModel;
using System.Xml.Serialization;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;

namespace MCRA.General {

    [Serializable]
    [XmlRoot("PbkModelSpecifications")]
    public class EmbeddedPbkModelSpecificationCollection : Collection<DeSolvePbkModelSpecification> {
    }
}
