namespace MCRA.General.Action.Settings.Dto {

    public class HumanMonitoringSettingsDto {

        public virtual List<string> SamplingMethodCodes { get; set; } = new();

        public virtual NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        public virtual double FractionOfLor { get; set; } = 1D;

        public virtual MissingValueImputationMethod MissingValueImputationMethod { get; set; }

        public virtual bool CorrelateTargetConcentrations { get; set; }

        public virtual NonDetectImputationMethod NonDetectImputationMethod { get; set; }

        public virtual bool ImputeHbmConcentrationsFromOtherMatrices { get; set; }

        public virtual double HbmBetweenMatrixConversionFactor { get; set; } = 1D;
        public virtual double MissingValueCutOff { get; set; } = 50D;

        public virtual bool StandardiseBlood { get; set; }

        public virtual StandardiseBloodMethod StandardiseBloodMethod { get; set; } = StandardiseBloodMethod.GravimetricAnalysis;

        public virtual bool StandardiseUrine { get; set; }
        
        public virtual StandardiseUrineMethod StandardiseUrineMethod { get; set; } = StandardiseUrineMethod.SpecificGravity;
    }
}
