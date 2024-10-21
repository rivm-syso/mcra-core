using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class HumanMonitoringAnalysisModuleConfig {
        [XmlIgnore]
        public virtual string HbmTargetMatrix {
            get => TargetMatrix.ToString();
            set => TargetMatrix = BiologicalMatrixConverter.FromString(value, BiologicalMatrix.Undefined, allowInvalidString: true);
        }
    }
}
