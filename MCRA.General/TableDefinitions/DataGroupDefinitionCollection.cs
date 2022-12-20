using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.TableDefinitions {

    [Serializable()]
    [XmlRoot("DataGroupDefinitions")]
    public class DataGroupDefinitionCollection : Collection<DataGroupDefinition> {

    }
}
