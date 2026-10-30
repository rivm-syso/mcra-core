using System.Xml;
using System.Xml.Serialization;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Commander.Models {
    public enum McraLoopType {
        Scoping,
        DataSource,
        Setting,
        Complex
    }
    /// <summary>
    /// Class to deserialize only the deserialized version info from
    /// a full loop settings XML
    /// </summary>
    [XmlRoot("LoopSettings")]
    public class LoopSettingsInfo {
        //structure holding version information of the MCRA version these settings
        //initialize with a new instance, containing only zeroes
        public virtual McraVersionInfo McraVersion { get; set; } = new();
    }

    /// <summary>
    /// Class to deserialize action settings that contain a scoping loop definition
    /// </summary>
    [XmlRoot("Project")]
    public class ActionLoopSettings {
        public HashSet<ScopingType> LoopScopingTypes { get; set; } = [];
    }

    /// <summary>
    /// Full class definition abstract class
    /// </summary>
    public class LoopSettings {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string Description { get; set; }
        public virtual McraVersionInfo McraVersion { get; set; } = new();
        public virtual McraLoopType LoopType { get; set; } = McraLoopType.Scoping;
        [XmlAnyElement]
        public virtual XmlElement LoopConfiguration { get; set; }
    }

    [XmlRoot(nameof(LoopSettings.LoopConfiguration))]
    public class ScopeLoopConfig {
        public ScopingType ScopingType { get; set; }
    }

    [XmlRoot(nameof(LoopSettings.LoopConfiguration))]
    public class DataSourceLoopConfig {
        public SourceTableGroup SourceTableGroup { get; set; }
    }

    [XmlRoot(nameof(LoopSettings.LoopConfiguration))]
    public class SettingLoopConfig {
        public ActionType ActionType { get; set; }
        public SettingsItemType SettingsItemType { get; set; }
        [XmlArrayItem("Value")]
        public string[] LoopValues { get; set; }
    }

    [XmlRoot(nameof(LoopSettings.LoopConfiguration))]
    public class ComplexLoopConfig {
        [XmlArrayItem("LoopItem")]
        public ComplexLoopItem[] LoopItems { get; set; }
    }
}
