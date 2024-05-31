using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General {

    public partial class McraUnitDefinitions {

        private static readonly object _instanceLocker = new();
        private static McraUnitDefinitions _instance;

        private static IDictionary<string, UnitDefinition> _unitDefinitions = null;

        public static readonly ConcentrationUnit DefaultExternalConcentrationUnit = ConcentrationUnit.mgPerKg;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraUnitDefinitions Instance {
            get {
                if (_instance == null) {
                    lock (_instanceLocker) {
                        _instance = new McraUnitDefinitions();
                    }
                }
                return _instance;
            }
        }

        private McraUnitDefinitions() {
            _unitDefinitions = _loadUnitDefinitions();
        }

        /// <summary>
        /// Returns a flat list of all table definitions.
        /// </summary>
        public IDictionary<string, UnitDefinition> UnitDefinitions {
            get {
                return _unitDefinitions;
            }
        }

        private static IDictionary<string, UnitDefinition> _loadUnitDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.UnitDefinitions.UnitDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(UnitDefinitionCollection));
                var result = (UnitDefinitionCollection)xs.Deserialize(stream);
                return result.ToDictionary(c => c.Id);
            }
        }
    }
}
