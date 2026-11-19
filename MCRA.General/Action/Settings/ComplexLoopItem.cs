using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {
    public class ComplexLoopItem {
        public string DisplayName { get; set; }
        public string ShortName { get; set; }

        [XmlArrayItem("Tag")]
        public virtual HashSet<string> Tags { get; set; }

        [XmlArrayItem("ModuleConfiguration")]
        public List<ModuleConfiguration> ModuleConfigurations { get; set; }

        [XmlArrayItem("DataSource")]
        public List<DataSourceMappingRecord> DataSources { get; set; }

        [XmlArrayItem("ScopeKeysFilter")]
        public List<ScopeKeysFilter> ScopeKeysFilters { get; set; }

    }
}
