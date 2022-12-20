using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General {

    [Serializable()]
    [XmlRoot("KineticModelDefinitions")]
    public class KineticModelDefinitionCollection : Collection<KineticModelDefinition> {
        
    }
}
