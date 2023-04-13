namespace MCRA.General.Action.Settings.Dto {

    public class EffectModelSettingsDto {

        public virtual RiskCalculationTier RiskCalculationTier { get; set; } = RiskCalculationTier.Custom;

        public virtual HealthEffectType HealthEffectType { get; set; } = HealthEffectType.Risk;

        public virtual double LeftMargin { get; set; } = 0.0001D;

        public virtual double RightMargin { get; set; } = 10000D;

        public virtual bool IsEAD { get; set; }

        public virtual double ThresholdMarginOfExposure { get; set; } = 1D;

        public virtual double ConfidenceInterval { get; set; } = 90D;

        public virtual double DefaultInterSpeciesFactorGeometricMean { get; set; } = 10D;

        public virtual double DefaultInterSpeciesFactorGeometricStandardDeviation { get; set; } = 1D;

        public virtual double DefaultIntraSpeciesFactor { get; set; } = 10D;

        public virtual int NumberOfLabels { get; set; } = 20;

        public virtual bool CumulativeRisk { get; set; }

        public virtual RiskMetricType RiskMetricType { get; set; } = RiskMetricType.MarginOfExposure;

        public virtual RiskMetricCalculationType RiskMetricCalculationType { get; set; } = RiskMetricCalculationType.RPFWeighted;

        public virtual double LeftMarginHI { get; set; } = 0.01D;

        public virtual int NumberOfSubstances { get; set; } = 20;

        public virtual SingleValueRiskCalculationMethod SingleValueRiskCalculationMethod { get; set; } = SingleValueRiskCalculationMethod.FromSingleValues;

        public virtual double Percentage { get; set; } = 0.1;

        public virtual bool IsInverseDistribution { get; set; }

        public virtual bool UseAdjustmentFactors { get; set; }

        public virtual AdjustmentFactorDistributionMethod ExposureAdjustmentFactorDistributionMethod { get; set; } = AdjustmentFactorDistributionMethod.Beta;

        public virtual AdjustmentFactorDistributionMethod HazardAdjustmentFactorDistributionMethod { get; set; } = AdjustmentFactorDistributionMethod.Beta;

        public virtual double ExposureParameterA { get; set; } = 2;
        public virtual double ExposureParameterB { get; set; } = 2;
        public virtual double ExposureParameterC { get; set; } = 0;
        public virtual double ExposureParameterD { get; set; } = 1;
        public virtual double HazardParameterA { get; set; } = 2;
        public virtual double HazardParameterB { get; set; } = 2;
        public virtual double HazardParameterC { get; set; } = 0;
        public virtual double HazardParameterD { get; set; } = 1;
        public virtual bool UseBackgroundAdjustmentFactor { get; set; }
        public virtual bool CalculateRisksByFood { get; set; }
    }
}
