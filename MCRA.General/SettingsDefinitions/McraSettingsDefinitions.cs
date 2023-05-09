using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.SettingsDefinitions {

    public class McraSettingsDefinitions {

        private static McraSettingsDefinitions _instance;

        private static IDictionary<SettingsItemType, SettingsItem> _settingsItems = null;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraSettingsDefinitions Instance {
            get { return _instance ?? (_instance = new McraSettingsDefinitions()); }
        }

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private McraSettingsDefinitions() {
            _settingsItems = loadDefinitions();
        }

        /// <summary>
        /// Returns all settings definitions.
        /// </summary>
        public IDictionary<SettingsItemType, SettingsItem> SettingsDefinitions {
            get {
                return _settingsItems;
            }
        }

        /// <summary>
        /// Returns all table group definitions (including the nested table definitions) as defined in
        /// the internal xml.
        /// </summary>
        private static IDictionary<SettingsItemType, SettingsItem> loadDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.SettingsDefinitions.SettingsDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(SettingsItemCollection));
                var definition = (SettingsItemCollection)xs.Deserialize(stream);
                var result = definition.ToDictionary(r => r.Id);
                return result;
            }
        }
    }
}
