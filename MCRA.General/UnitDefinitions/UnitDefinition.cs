using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MCRA.General {
    public class UnitDefinition {

        public string Id { get; set; }

        public string Name { get; set; }

        [XmlArrayItem("Unit")]
        public List<UnitValueDefinition> Units { get; set; }

        public T FromString<T>(string str) {
            var unit = Units.FirstOrDefault(r => r.AcceptsFormat(str));
            if (unit == null) {
                throw new Exception($"Unknown unit specification: '{str}' is not a valid specification of the unit {typeof(T).Name}.");
            }
            return (T)Enum.Parse(typeof(T), unit.Id);
        }

        public T TryGetFromString<T>(string str, T defaultUnit) {
            var unit = Units.FirstOrDefault(r => r.AcceptsFormat(str));
            if (unit == null) {
                return defaultUnit;
            }
            return (T)Enum.Parse(typeof(T), unit.Id);
        }

        public UnitValueDefinition GetUnitValueDefinition<T>(T unit) {
            return Units.First(r => r.Id == unit.ToString());
        }
    }
}
