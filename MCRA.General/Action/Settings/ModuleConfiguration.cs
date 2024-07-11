using System.Xml.Serialization;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.Settings {
    public sealed class ModuleConfiguration {
        [XmlAttribute("module")]
        public string ActionTypeString { get; set; }

        [XmlIgnore]
        public ActionType ActionType {
            get => Enum.TryParse<ActionType>(ActionTypeString, true, out var actionType)
                ? actionType
                : ActionType.Unknown;
            set => ActionTypeString = value.ToString();
        }

        [XmlArrayItem("Setting")]
        public ModuleSetting[] Settings {
            get => SettingsDictionary.Values.OrderBy(s => s.Id.ToString()).ToArray();
            set => SettingsDictionary = value
                .Where(v => v.Id != SettingsItemType.Undefined)
                .ToDictionary(v => v.Id);
        }
        [XmlIgnore]
        public Dictionary<SettingsItemType, ModuleSetting> SettingsDictionary { get; set; } = new();

        public override string ToString() {
            return $"{ActionType}";
        }
    }
}
