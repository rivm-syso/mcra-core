using System.Xml.Serialization;

namespace MCRA.General.ScopingTypeDefinitions {
    public class ParentScopeReference {
        public string IdField { get; set; }

        public ScopingType ReferencedScope { get; set; }

        [XmlAttribute(AttributeName = "IsKeysList")]
        public bool IsKeysList { get; set; }

        [XmlAttribute(AttributeName = "MatchAny")]
        public bool MatchAny { get; set; }

    }
}
