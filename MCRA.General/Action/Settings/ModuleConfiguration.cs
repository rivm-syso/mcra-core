using System.Text.Json.Serialization;
using System.Xml.Serialization;
using MCRA.General.ActionSettingsTemplates;
using MCRA.General.SettingsDefinitions;

namespace MCRA.General.Action.Settings {
    public sealed class ModuleConfiguration {
        [XmlAttribute("module")]
        public ActionType ActionType { get; set; }
        [XmlAttribute("version")]
        public string ModuleVersion { get; set; } = $"{ThisAssembly.Git.BaseVersion.Major}.{ThisAssembly.Git.BaseVersion.Minor}.{ThisAssembly.Git.BaseVersion.Patch}";

        [XmlArrayItem("Setting")]
        public ModuleSetting[] Settings {
            get => SettingsDictionary.Values.OrderBy(s => s.Id.ToString()).ToArray();
            set => SettingsDictionary = value.ToDictionary(v => v.Id);
        }
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<SettingsItemType, ModuleSetting> SettingsDictionary { get; set; } = new();

        public override string ToString() {
            return $"{ActionType} {ModuleVersion}";
        }
    }
}
