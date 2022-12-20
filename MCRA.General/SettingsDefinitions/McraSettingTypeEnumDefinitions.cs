using MCRA.General;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.SettingsDefinitions {

    public class McraSettingTypeEnumDefinitions {

        private static readonly object _instanceLocker = new object();
        private static McraSettingTypeEnumDefinitions _instance;

        private static IDictionary<string, UnitDefinition> _settingTypeEnumDefinitions = null;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraSettingTypeEnumDefinitions Instance {
            get {
                if (_instance == null) {
                    lock (_instanceLocker) {
                        _instance = new McraSettingTypeEnumDefinitions();
                    }
                }
                return _instance;
            }
        }

        private McraSettingTypeEnumDefinitions() {
            _settingTypeEnumDefinitions = _loadSettingTypeEnumDefinitions();
        }

        /// <summary>
        /// Returns a flat list of all table definitions.
        /// </summary>
        public IDictionary<string, UnitDefinition> SettingTypeEnumDefinitions {
            get {
                return _settingTypeEnumDefinitions;
            }
        }

        private static IDictionary<string, UnitDefinition> _loadSettingTypeEnumDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.SettingsDefinitions.SettingTypeEnumDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(UnitDefinitionCollection));
                var definition = (UnitDefinitionCollection)xs.Deserialize(stream);
                var result = definition.ToDictionary(r => r.Id);
                return result;
            }
        }
    }
}
