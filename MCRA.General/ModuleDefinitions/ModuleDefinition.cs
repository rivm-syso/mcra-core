using MCRA.General.SettingsDefinitions;
using System.Xml.Serialization;

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

        [XmlArrayItem("Tier")]
        public List<ModuleTier> Tiers { get; set; }

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
                if (!string.IsNullOrEmpty(TableGroup)) {
                    return (SourceTableGroup)Enum.Parse(typeof(SourceTableGroup), TableGroup);
                }
                return SourceTableGroup.Unknown;
            }
        }

        public List<SettingsItemType> SelectionSettingsItems {
            get {
                if (SelectionSettings?.Any() ?? false) {
                    return SelectionSettings
                        .Select(r => (SettingsItemType)Enum.Parse(typeof(SettingsItemType), r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> CalculationSettingsItems {
            get {
                if (CalculationSettings?.Any() ?? false) {
                    return CalculationSettings
                        .Select(r => (SettingsItemType)Enum.Parse(typeof(SettingsItemType), r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> UncertaintySettingsItems {
            get {
                if (UncertaintySettings?.Any() ?? false) {
                    return UncertaintySettings
                        .Select(r => (SettingsItemType)Enum.Parse(typeof(SettingsItemType), r))
                        .ToList();
                }
                return new List<SettingsItemType>();
            }
        }

        public List<SettingsItemType> OutputSettingsItems {
            get {
                if (OutputSettings?.Any() ?? false) {
                    return OutputSettings
                        .Select(r => (SettingsItemType)Enum.Parse(typeof(SettingsItemType), r))
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
    }
}
