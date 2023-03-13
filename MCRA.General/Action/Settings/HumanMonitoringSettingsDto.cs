namespace MCRA.General.Action.Settings.Dto {

    public class HumanMonitoringSettingsDto {

        public virtual List<string> SurveyCodes { get; set; } = new List<string>();

        public virtual List<string> SamplingMethodCodes { get; set; } = new List<string>();

        public virtual NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        public virtual double FractionOfLor { get; set; } = 1D;

        public virtual MissingValueImputationMethod MissingValueImputationMethod { get; set; }

        public virtual bool CorrelateTargetConcentrations { get; set; }

        public virtual NonDetectImputationMethod NonDetectImputationMethod { get; set; }

        public bool ImputeHbmConcentrationsFromOtherMatrices { get; set; }

        public double HbmBetweenMatrixConversionFactor { get; set; } = 1D;

        public double MissingValueCutOff { get; set; } = 50D;

    }
}
