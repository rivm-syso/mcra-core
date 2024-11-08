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

        public IDictionary<SettingsTemplateType, SettingsTemplate> TemplateSettings =>
            McraTemplatesCollection.Instance.GetTiers(ActionType)?.ToDictionary(t => t.Tier);

        public HashSet<ActionType> PrimaryEntities =>
            Entities.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType).ToHashSet();

        public HashSet<ActionType> DataInputs =>
            SelectionInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType).ToHashSet();

        public HashSet<ActionType> CalculatorInputs =>
            CalculationInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType).ToHashSet();

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
                if (SelectionSettings?.Count > 0) {
                    return SelectionSettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> CalculationSettingsItems {
            get {
                if (CalculationSettings?.Count > 0) {
                    return CalculationSettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> UncertaintySettingsItems {
            get {
                if (UncertaintySettings?.Count > 0) {
                    return UncertaintySettings
                        .Select(r => Enum.Parse<SettingsItemType>(r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> OutputSettingsItems {
            get {
                if (OutputSettings?.Count > 0) {
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
            var result = new HashSet<SettingsItemType>();
            var template = McraTemplatesCollection.Instance.GetTemplate(selectedTier);

            if (recursive) {
                result = template.ModuleConfigurations.SelectMany(s => s.Settings)
                    .Select(s => s.Id)
                    .ToHashSet();
            } else {
                if (template.ConfigurationsDictionary.TryGetValue(ActionType, out var config)) {
                    result = config.Settings.Select(s => s.Id).ToHashSet();
                }
            }
            return result;
        }
    }
}
