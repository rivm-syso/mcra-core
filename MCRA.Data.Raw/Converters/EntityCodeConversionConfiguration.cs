using MCRA.Utils.Xml;
using System.Xml.Serialization;

namespace MCRA.Data.Raw.Converters {
    public class EntityCodeConversionConfiguration {
        [XmlElement("EntityCodeConversions")]
        public EntityCodeConversionsCollection[] EntityCodeConversions;

        public static EntityCodeConversionConfiguration FromXmlFile(string filename) {
            var result = XmlSerialization.FromXmlFile<EntityCodeConversionConfiguration>(filename);
            var xmlBaseDir = Path.GetDirectoryName(filename);
            foreach (var record in result.EntityCodeConversions) {
                if ((!record.ConversionTuples?.Any() ?? false) && !string.IsNullOrEmpty(record.RecodingsFileName)) {
                    record.LoadFromFile(xmlBaseDir);
                }
            }
            return result;
        }
    }
}
