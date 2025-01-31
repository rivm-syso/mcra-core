namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class RisksModuleConfig {
        public double[] RiskPercentiles {
            get {
                return RiskMetricType == RiskMetricType.HazardExposureRatio
                    ? SelectedPercentiles
                        .Select(c => 100 - c).Reverse().ToArray()
                    : SelectedPercentiles.ToArray();
            }
        }

        public bool IsCumulative => MultipleSubstances && CumulativeRisk;
    }
}
