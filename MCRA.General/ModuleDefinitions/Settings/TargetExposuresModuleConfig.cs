using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class TargetExposuresModuleConfig {
        [XmlIgnore]
        public bool Aggregate {
            get => ExposureSources.Contains(ExposureSource.OtherNonDietary);
            set => Aggregate = value;
        } 
    }
}
