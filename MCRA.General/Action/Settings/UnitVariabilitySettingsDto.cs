namespace MCRA.General.Action.Settings.Dto {

    public class UnitVariabilitySettingsDto {

        public virtual bool UseUnitVariability { get; set; } = false;

        public virtual UnitVariabilityModelType UnitVariabilityModel { get; set; } = UnitVariabilityModelType.LogNormalDistribution;

        public virtual EstimatesNature EstimatesNature { get; set; } = EstimatesNature.Realistic;

        public virtual UnitVariabilityType UnitVariabilityType { get; set; } = UnitVariabilityType.VariabilityFactor;

        public virtual MeanValueCorrectionType MeanValueCorrectionType { get; set; } = MeanValueCorrectionType.Unbiased;

        public virtual int DefaultFactorLow { get; set; } = 1;

        public virtual int DefaultFactorMid { get; set; } = 5;

        public virtual UnitVariabilityCorrelationType CorrelationType { get; set; } = UnitVariabilityCorrelationType.NoCorrelation;
    }
}
