using System.Xml.Serialization;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.ModuleDefinitions {
    public sealed class ModuleDefinition {

        private string _shortDescription { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string ShortDescription {
            get {
                if (!string.IsNullOrEmpty(_shortDescription)) {
                    return _shortDescription;
                }
                return Description;
            }
            set {
                _shortDescription = value;
            }
        }

        public ActionType ActionType { get; set; } = ActionType.Unknown;
        public ModuleType ModuleType { get; set; }
        public bool CanCompute { get; set; } = false;
        public string TableGroup { get; set; }

        public bool IsAdvancedActionType { get; set; }

        public string DataDescription { get; set; }
        public string CalculationDescription { get; set; }
        public bool AllowMultipleDataSources { get; set; }
        public bool AllowDefaultData { get; set; }

        public bool HasUncertaintyAnalysis { get; set; }
        public bool HasUncertaintyFactorial { get; set; }

        [XmlArrayItem("SelectionSetting")]
        public List<string> SelectionSettings { get; set; }

        [XmlArrayItem("CalculationSetting")]
        public List<string> CalculationSettings { get; set; }

        [XmlArrayItem("OutputSetting")]
        public List<string> OutputSettings { get; set; }

        [XmlArrayItem("UncertaintySetting")]
        public List<string> UncertaintySettings { get; set; }

        [XmlArrayItem("Entity")]
        public HashSet<string> Entities { get; set; }

        [XmlArrayItem("Input")]
        public HashSet<string> SelectionInputs { get; set; }

        [XmlArrayItem("Input")]
        public HashSet<string> CalculationInputs { get; set; }

        [XmlArrayItem("LoopEntity")]
        public HashSet<ScopingType> LoopEntities { get; set; }

        [XmlArrayItem("UncertaintySource")]
        public List<UncertaintySource> UncertaintySources { get; set; }

        public string TierSelectionSetting { get; set; }

        public IDictionary<SettingsTemplateType, SettingsTemplate> TemplateSettings =>
            McraTemplatesCollection.Instance.GetModuleTemplate(ActionType);

        public HashSet<ActionType> PrimaryEntities {
            get {
                var entities = Entities.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType);
                return new HashSet<ActionType>(entities);
            }
        }

        public HashSet<ActionType> DataInputs {
            get {
                var entities = SelectionInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType);
                return new HashSet<ActionType>(entities);
            }
        }

        public HashSet<ActionType> CalculatorInputs {
            get {
                var entities = CalculationInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType);
                return new HashSet<ActionType>(entities);
            }
        }

        public List<ActionType> Inputs {
            get {
                return DataInputs
                    .Union(CalculatorInputs)
                    .ToList();
            }
        }

        public SourceTableGroup SourceTableGroup {
            get {
                if (!string.IsNullOrEmpty(TableGroup) && Enum.TryParse<SourceTableGroup>(TableGroup, out var tableGroup)) {
                    return tableGroup;
                }
                return SourceTableGroup.Unknown;
            }
        }

        public List<SettingsItemType> SelectionSettingsItems {
            get {
                if (SelectionSettings?.Any() ?? false) {
                    return SelectionSettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> CalculationSettingsItems {
            get {
                if (CalculationSettings?.Any() ?? false) {
                    return CalculationSettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> UncertaintySettingsItems {
            get {
                if (UncertaintySettings?.Any() ?? false) {
                    return UncertaintySettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> OutputSettingsItems {
            get {
                if (OutputSettings?.Any() ?? false) {
                    return OutputSettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> AllModuleSettings {
            get {
                var result = new List<SettingsItemType>();
                result.AddRange(SelectionSettingsItems);
                result.AddRange(CalculationSettingsItems);
                result.AddRange(OutputSettingsItems);
                result.AddRange(UncertaintySettingsItems);
                return result;
            }
        }

        /// <summary>
        /// Gets all settings in tiers and sub tiers
        /// </summary>
        /// <param name="selectedTier"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public ICollection<SettingsItemType> GetAllTierSettings(
            SettingsTemplateType selectedTier,
            bool recursive
        ) {
            HashSet<SettingsItemType> result = new();
            if(TemplateSettings?.TryGetValue(selectedTier, out var tierSettings) ?? false) {
                result = tierSettings.Settings.Select(s => s.Id).ToHashSet();

                if (recursive && Inputs.Any()) {
                    foreach (var input in Inputs) {
                        var inputModule = McraModuleDefinitions.Instance.ModuleDefinitions[input];
                        result.UnionWith(inputModule.GetAllTierSettings(selectedTier, recursive));
                    }
                }
            }
            return result;
        }
    }
}
