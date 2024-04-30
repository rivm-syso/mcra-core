using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.ModuleDefinitions {

    public class McraModuleDefinitions {

        private static McraModuleDefinitions _instance;

        private static IDictionary<ActionType, ModuleDefinition> _moduleDefinitions = null;
        private static IDictionary<string, ModuleDefinition> _moduleDefinitionsById = null;
        private static IDictionary<SourceTableGroup, ModuleDefinition> _moduleDefinitionsByTableGroup = null;
        private static IDictionary<SettingsItemType, ModuleDefinition[]> _modulesPerSettingType = null;
        private static Collection<ModuleGroupDefinition> _moduleGroupDefinitions;
        private static Dictionary<ActionType, ActionClass> _actionClassLookup;

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraModuleDefinitions Instance {
            get { return _instance ??= new McraModuleDefinitions(); }
        }

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private McraModuleDefinitions() {
            _moduleGroupDefinitions = _loadModuleDefinitions();
        }

        /// <summary>
        /// Returns all module group definitions by the module definitions xml.
        /// </summary>
        public Collection<ModuleGroupDefinition> ModuleGroupDefinitions {
            get {
                return _moduleGroupDefinitions;
            }
        }

        /// <summary>
        /// Returns all module definitions.
        /// </summary>
        public IDictionary<ActionType, ModuleDefinition> ModuleDefinitions {
            get {
                if (_moduleDefinitions == null) {
                    _moduleDefinitions = _moduleGroupDefinitions
                        .SelectMany(r => r.Modules)
                        .ToDictionary(r => r.ActionType);
                }
                return _moduleDefinitions;
            }
        }

        /// <summary>
        /// Returns all module definitions by ID.
        /// </summary>
        public IDictionary<string, ModuleDefinition> ModuleDefinitionsById {
            get {
                if (_moduleDefinitionsById == null) {
                    _moduleDefinitionsById = _moduleGroupDefinitions
                        .SelectMany(r => r.Modules)
                        .ToDictionary(r => r.Id);
                }
                return _moduleDefinitionsById;
            }
        }

        /// <summary>
        /// Returns all module definitions by ID.
        /// </summary>
        public IDictionary<SettingsItemType, ModuleDefinition[]> ModuleDefinitionsBySettingType {
            get {
                if (_modulesPerSettingType == null) {
                    _modulesPerSettingType = _moduleGroupDefinitions
                        .SelectMany(r => r.Modules)
                        .SelectMany(r => r.AllModuleSettings, (m, s) => new { SettingType = s, ModuleDef = m })
                        .GroupBy(g => g.SettingType, g => g.ModuleDef)
                        .ToDictionary(r => r.Key, r => r.ToArray());
                }
                return _modulesPerSettingType;
            }
        }

        /// <summary>
        /// Returns all module definitions by ID.
        /// </summary>
        public IDictionary<SourceTableGroup, ModuleDefinition> ModuleDefinitionsByTableGroup {
            get {
                if (_moduleDefinitionsByTableGroup == null) {
                    _moduleDefinitionsByTableGroup = _moduleGroupDefinitions
                        .SelectMany(r => r.Modules)
                        .Where(r => r.SourceTableGroup != SourceTableGroup.Unknown)
                        .ToDictionary(r => r.SourceTableGroup);
                }
                return _moduleDefinitionsByTableGroup;
            }
        }

        /// <summary>
        /// Returns the action class of the action type.
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public ActionClass GetActionClass(ActionType actionType) {
            if (_actionClassLookup == null) {
                _actionClassLookup = ModuleGroupDefinitions
                    .SelectMany(r => r.Modules, (c, m) => new { c.ActionClass, m.ActionType })
                    .ToDictionary(r => r.ActionType, r => r.ActionClass);
            }
            return _actionClassLookup[actionType];
        }

        /// <summary>
        /// Returns all table group definitions (including the nested table definitions) as defined in
        /// the internal xml.
        /// </summary>
        private static Collection<ModuleGroupDefinition> _loadModuleDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.ModuleDefinitions.ModuleDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(ModuleGroupDefinitionCollection));
                var definitions = (ModuleGroupDefinitionCollection)xs.Deserialize(stream);
                return definitions;
            }
        }
    }
}
