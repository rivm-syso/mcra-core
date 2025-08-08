using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class HumanMonitoringAnalysisModuleConfig {
        [XmlIgnore]
        public virtual string HbmTargetMatrix {
            get => TargetMatrix.ToString();
            set => TargetMatrix = BiologicalMatrixConverter.FromString(value, BiologicalMatrix.Undefined);
        }
        public int NonStationaryPeriodInDays { get; set; }

        public bool UseKineticConversionFactorModels() {
            return ApplyKineticConversions
                && (HbmKineticConversionMethod == KineticConversionMethodType.ConversionFactorModel
                    || HbmKineticConversionMethod == KineticConversionMethodType.PbkWithFallback
                    || !HbmConvertToSingleTargetMatrix // For now, multiple target conversion always requires KCF models
                );
        }

        public bool UsePbkModels() {
            return ApplyKineticConversions
                && (HbmKineticConversionMethod == KineticConversionMethodType.PbkModel
                    || HbmKineticConversionMethod == KineticConversionMethodType.PbkWithFallback)
                && HbmConvertToSingleTargetMatrix; // For now, only supported for conversion to single target
        }
    }
}
