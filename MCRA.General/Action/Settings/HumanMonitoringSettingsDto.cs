using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    public class HumanMonitoringSettingsDto {

        public virtual List<string> SamplingMethodCodes { get; set; } = new();

        public virtual NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        public virtual double FractionOfLor { get; set; } = 1D;

        public virtual NonDetectImputationMethod NonDetectImputationMethod { get; set; }

        public virtual MissingValueImputationMethod MissingValueImputationMethod { get; set; }

        public virtual double MissingValueCutOff { get; set; } = 50D;

        public virtual bool CorrelateTargetConcentrations { get; set; }


        [XmlIgnore]
        public virtual BiologicalMatrix TargetMatrix { get; set; }

        public virtual string HbmTargetMatrix {
            get {
                return TargetMatrix.ToString();
            }
            set {
                TargetMatrix = BiologicalMatrixConverter.TryGetFromString(value, BiologicalMatrix.Undefined);
            }
        }

        public virtual bool ImputeHbmConcentrationsFromOtherMatrices { get; set; }

        public virtual double HbmBetweenMatrixConversionFactor { get; set; } = 1D;

        public virtual bool StandardiseBlood { get; set; }

        public virtual StandardiseBloodMethod StandardiseBloodMethod { get; set; } = StandardiseBloodMethod.GravimetricAnalysis;

        public virtual bool StandardiseBloodExcludeSubstances { get; set; }

        public virtual List<string> StandardiseBloodExcludedSubstancesSubset { get; set; } = new();

        public virtual bool StandardiseUrine { get; set; }

        public virtual StandardiseUrineMethod StandardiseUrineMethod { get; set; } = StandardiseUrineMethod.SpecificGravity;

        public virtual KineticConversionType KineticConversionMethod { get; set; } = KineticConversionType.Default;

        public virtual bool StandardiseUrineExcludeSubstances { get; set; }

        public virtual List<string> StandardiseUrineExcludedSubstancesSubset { get; set; } = new();
    }
}
