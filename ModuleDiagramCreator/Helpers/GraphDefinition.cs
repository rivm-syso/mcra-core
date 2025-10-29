using System.Xml.Serialization;
using MCRA.General;

namespace ModuleDiagramCreator.Helpers {
    public class GraphDefinition {
        [XmlAttribute]
        public ActionType ActionType;
        [XmlAttribute]
        public int X;
        [XmlAttribute]
        public int Y;
        [XmlAttribute]
        public int Linebreaks;
    }
}
