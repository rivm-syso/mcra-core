using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCRA.General.ScopingTypeDefinitions {

    [XmlRoot("ScopingTypeDefinitions")]
    [XmlType(TypeName = "ScopingTypeDefinition")]
    public class ScopingTypeDefinition {
        public ScopingType Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SourceTableGroup TableGroup { get; set; }
        public RawDataSourceTableID? RawTableId { get; set; }
        public bool IsStrongEntity { get; set; }
        public bool AllowUserSelection { get; set; }
        public bool IsAutoScope { get; set; }

        [XmlArrayItem("ParentScopeReference")]
        public List<ParentScopeReference> ParentScopeReferences { get; set; }
    }
}
