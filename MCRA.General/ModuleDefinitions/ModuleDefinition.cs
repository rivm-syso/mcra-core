using System.Xml.Serialization;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.ModuleDefinitions {

    public abstract class ModuleSettingItem {
        [XmlAttribute("source")]
        public string Source { get; set; }
        [XmlText]
        public string Setting { get; set; }
    }
    public class SelectionSetting : ModuleSettingItem { }
    public class CalculationSetting : ModuleSettingItem { }
    public class OutputSetting : ModuleSettingItem { }
    public class UncertaintySetting : ModuleSettingItem { }


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
        public bool DefaultCompute { get; set; } = false;
        public string TableGroup { get; set; }

        public bool IsAdvancedActionType { get; set; }

        public string DataDescription { get; set; }
        public string CalculationDescription { get; set; }
        public bool AllowMultipleDataSources { get; set; }
        public bool AllowDefaultData { get; set; }

        public bool HasUncertaintyAnalysis { get; set; }
        public bool HasUncertaintyFactorial { get; set; }

        public HashSet<SelectionSetting> SelectionSettings { get; set; }

        public HashSet<CalculationSetting> CalculationSettings { get; set; }

        public HashSet<OutputSetting> OutputSettings { get; set; }

        public HashSet<UncertaintySetting> UncertaintySettings { get; set; }

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
            [.. Entities.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType)];

        public HashSet<ActionType> DataInputs =>
            [.. SelectionInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType)];

        public HashSet<ActionType> CalculatorInputs =>
            [.. CalculationInputs.Select(r => McraModuleDefinitions.Instance.ModuleDefinitionsById[r].ActionType)];

        public List<ActionType> Inputs {
            get {
                return [.. DataInputs.Union(CalculatorInputs)];
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

        public ICollection<SettingsItemType> SelectionSettingsItems =>
            [.. SelectionSettings?.Select(r => Enum.Parse<SettingsItemType>(r.Setting)) ?? []];

        public ICollection<SettingsItemType> CalculationSettingsItems =>
            [.. CalculationSettings?.Select(r => Enum.Parse<SettingsItemType>(r.Setting)) ?? []];

        public ICollection<SettingsItemType> UncertaintySettingsItems =>
            [.. UncertaintySettings?.Select(r => Enum.Parse<SettingsItemType>(r.Setting)) ?? []];

        public ICollection<SettingsItemType> OutputSettingsItems =>
            [.. OutputSettings?.Select(r => Enum.Parse<SettingsItemType>(r.Setting)) ?? []];

        public ICollection<SettingsItemType> AllModuleSettings =>
            [.. SelectionSettingsItems
                .Concat(CalculationSettingsItems)
                .Concat(UncertaintySettingsItems)
                .Concat(OutputSettingsItems)
            ];

        private HashSet<ActionType> _sourceActionTypes;
        public ICollection<ActionType> SourceActionTypes {
            get {
                if (_sourceActionTypes == null) {
                    var settings = SelectionSettings.Select(r => r.Source)
                        .Concat(CalculationSettings.Select(r => r.Source))
                        .Concat(OutputSettings.Select(r => r.Source))
                        .Concat(UncertaintySettings.Select(r => r.Source));

                    _sourceActionTypes = [.. settings
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Select(Enum.Parse<ActionType>)
                    ];
                }
                return _sourceActionTypes;
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
                result = [.. template.ModuleConfigurations.SelectMany(s => s.Settings).Select(s => s.Id)];
            } else {
                if (template.ConfigurationsDictionary.TryGetValue(ActionType, out var config)) {
                    result = [.. config.Settings.Select(s => s.Id)];
                }
            }
            return result;
        }
    }
}
