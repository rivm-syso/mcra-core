using System.Xml.Serialization;

namespace MCRA.General {
    public class UnitDefinition {

        public string Id { get; set; }

        public string Name { get; set; }

        [XmlArrayItem("Unit")]
        public List<UnitValueDefinition> Units { get; set; }

        [XmlArrayItem("Alias")]
        public HashSet<string> UndefinedAliases { get; set; }

        [XmlArray("URIs")]
        [XmlArrayItem("URI")]
        public HashSet<string> Uris { get; set; }

        public T FromString<T>(string str, bool allowInvalidString = false, T defaultUnit = default) {
            var unit = Units.FirstOrDefault(r => r.AcceptsFormat(str));
            if (unit != null && Enum.TryParse(typeof(T), unit.Id, out var result)) {
                return (T)result;
            } else if (allowInvalidString) {
                return defaultUnit;
            }
            throw new Exception($"Unknown unit specification: '{str}' is not a valid specification of the unit {typeof(T).Name}.");
        }

        public T FromUri<T>(string str, bool allowInvalidString = false, T defaultUnit = default) {
            var unit = Units.FirstOrDefault(r => r.AcceptsUri(str));
            if (unit != null && Enum.TryParse(typeof(T), unit.Id, out var result)) {
                return (T)result;
            } else if (allowInvalidString) {
                return defaultUnit;
            }
            throw new Exception($"Unknown URI: '{str}' is not a valid specification of {typeof(T).Name}.");
        }

        public UnitValueDefinition GetUnitValueDefinition<T>(T unit) {
            return Units.First(r => r.Id == unit.ToString());
        }
    }
}
