using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.Settings {

    #region Classes - BackwardCompatibility
    public class SelectedCompoundDto {
        public virtual string CodeCompound {
            get;
            set;
        }
    }

    /// <summary>
    /// Class to get only the deserialized version info from
    /// a project XML string
    /// </summary>
    [XmlRoot("Project")]
    public class ProjectVersionInfo {
        //structure holding version information of the MCRA version these settings
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new();
    }
    #endregion

    [XmlRoot("Project")]
    public partial class ProjectDto {
        #region Simple Properties

        //structure holding version information of the MCRA version these settings
        //were saved with
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new();

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime DateCreated { get; set; }
        public virtual DateTime DateModified { get; set; }
        public virtual ActionType ActionType { get; set; } = ActionType.Unknown;
        public virtual string StandardActionCode { get; set; } = null;
        public virtual string ShortOutputTemplate { get; set; } = null;
        public virtual string DefaultTaskName { get; set; } = null;

        #endregion
        private Dictionary<ActionType, ModuleConfigBase> _moduleConfigsDictionary = new();

        public ProjectDto() { }

        public ProjectDto(params ModuleConfigBase[] configs) {
            if (configs.Any()) {
                ActionType = configs[0].ActionType;
            }
            foreach (var c in configs) {
                SaveModuleConfiguration(c);
            }
        }

        #region Collection Properties
        [XmlArrayItem("ActionType")]
        public virtual HashSet<ActionType> CalculationActionTypes { get; set; } = new();

        [XmlArrayItem("ScopeKeysFilter")]
        public virtual List<ScopeKeysFilter> ScopeKeysFilters { get; set; } = new();

        public virtual HashSet<ScopingType> LoopScopingTypes { get; set; } = new();
        #endregion

        #region Methods
        public HashSet<string> GetFilterCodes(ScopingType scopingType) =>
            ScopeKeysFilters?
                .FirstOrDefault(r => r.ScopingType == scopingType)?
                .SelectedCodes;

        /// <summary>
        /// Sets the scope keys filter for the specified scoping type.
        /// </summary>
        /// <param name="scopingType"></param>
        /// <param name="codes"></param>
        public void SetFilterCodes(ScopingType scopingType, IEnumerable<string> codes) {
            var scopeKeysFilter = ScopeKeysFilters?.FirstOrDefault(r => r.ScopingType == scopingType);
            if (scopeKeysFilter == null) {
                scopeKeysFilter = new ScopeKeysFilter {
                    ScopingType = scopingType
                };
                ScopeKeysFilters.Add(scopeKeysFilter);
            }
            scopeKeysFilter?.SetCodesScope(codes);
        }

        public void AddCalculationAction(ActionType actionInputType) {
            CalculationActionTypes.Add(actionInputType);
        }

        public void RemoveCalculationAction(ActionType actionInputType) {
            CalculationActionTypes?.Remove(actionInputType);
        }

        [XmlIgnore]
        [JsonIgnore]
        public IDictionary<SourceTableGroup, List<IRawDataSourceVersion>> ProjectDataSourceVersions { get; set; } = null;

        #region ModuleSettingsNewStyle
        [XmlArrayItem("ModuleConfiguration")]
        public ModuleConfiguration[] ModuleConfigurations {
            //Skip module configurations in which all settings come from a different source module
            get => _moduleConfigsDictionary.Values
                .Select(s => s.AsConfiguration())
                .Where(c => c.SettingsDictionary.Count > 0)
                .OrderBy(m => m.ActionType.ToString())
                .ToArray();
            set => _moduleConfigsDictionary = value
                .Where(v => v.ActionType != ActionType.Unknown)
                .ToDictionary(v => v.ActionType, v => ModuleConfigBase.Create(this, v));
        }

        public ModuleConfigBase GetModuleConfiguration(ActionType actionType) {
            if (actionType == ActionType.Unknown) return null;

            if (!_moduleConfigsDictionary.TryGetValue(actionType, out var config)) {
                config = ModuleConfigBase.Create(this, actionType);
                if (config != null) {
                    _moduleConfigsDictionary[actionType] = config;
                }
            }
            return config;
        }

        public T GetModuleConfiguration<T>() where T : ModuleConfigBase, new() {
            var actionType = new T().ActionType;
            return (T)GetModuleConfiguration(actionType);
        }

        public void SetRawValue(ActionType actionType, SettingsItemType settingType, string value) {
            var config = GetModuleConfiguration(actionType);
            config?.Apply(settingType, value);
        }

        public object GetRawValue(ActionType actionType, SettingsItemType settingType) {
            var rawConfig = GetModuleConfiguration(actionType);
            return rawConfig?.GetValue(settingType);
        }

        public void SaveModuleConfiguration(ModuleConfigBase config) {
            _moduleConfigsDictionary[config.ActionType] = config;
        }

        public ModuleConfigBase ApplySettings(ModuleConfiguration settings) {
            if (_moduleConfigsDictionary.TryGetValue(settings.ActionType, out var config)) {
                config.Apply(settings);
            } else {
                config = ModuleConfigBase.Create(this, settings.ActionType);
                if (config != null) {
                    _moduleConfigsDictionary[settings.ActionType] = config;
                }
            }
            return config;
        }

        public ModuleConfigBase ApplySettings(ActionType actionType, IEnumerable<ModuleSetting> settings) {
            var moduleConfig = new ModuleConfiguration {
                ActionType = actionType,
                SettingsDictionary = settings?.ToDictionary(s => s.Id) ?? new()
            };
            var config = ApplySettings(moduleConfig);
            return config;
        }
        #endregion

        /// <summary>
        /// Reconstructs a DataSourceConfiguration object from the tables in the compiled datasource
        /// </summary>
        public DataSourceConfiguration GetDataSourceConfiguration() {
            var dsConfig = new DataSourceConfiguration();
            if (ProjectDataSourceVersions != null) {
                dsConfig.DataSourceMappingRecords = ProjectDataSourceVersions
                    .SelectMany(r => r.Value, (kvp, v) => new DataSourceMappingRecord() {
                        IdRawDataSourceVersion = v.id,
                        SourceTableGroup = kvp.Key,
                        Name = v.Name,
                        RawDataSourcePath = v.DataSourcePath,
                        Checksum = v.Checksum
                    }).ToList();
            }
            return dsConfig;
        }
        #endregion
    }
}
