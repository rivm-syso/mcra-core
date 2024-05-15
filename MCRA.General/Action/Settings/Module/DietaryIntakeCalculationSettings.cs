namespace MCRA.General.Action.Settings {

    public class DietaryIntakeCalculationSettings {

        public virtual SettingsTemplateType DietaryIntakeCalculationTier { get; set; }

        public virtual bool ImputeExposureDistributions { get; set; }

        public virtual DietaryExposuresDetailsLevel DietaryExposuresDetailsLevel { get; set; }

        public virtual SingleValueDietaryExposuresCalculationMethod SingleValueDietaryExposureCalculationMethod { get; set; } = SingleValueDietaryExposuresCalculationMethod.IESTI;

        public virtual bool VariabilityDiagnosticsAnalysis { get; set; }

        public virtual bool AnalyseMcr { get; set; }

        public virtual ExposureApproachType ExposureApproachType { get; set; } = ExposureApproachType.RiskBased;
    }
}
