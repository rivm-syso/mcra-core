using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Summarizes info for the positives amounts model like variance
    /// components, estimates and exposure transformer.
    /// </summary>
    public sealed class NormalAmountsModelSummary : AmountsModelSummary {
        public ParameterEstimates VarianceBetween { get; set; }
        public ParameterEstimates VarianceWithin { get; set; }
        public double VarianceDistribution { get; set; }
        public double DegreesOfFreedom { get; set; }
        public double _2LogLikelihood { get; set; }
        public List<double> Residuals { get; set; }
        public List<double> Blups { get; set; }
        public List<ParameterEstimates> AmountModelEstimates { get; set; }
        public IntakeTransformer IntakeTransformer { get; set; }
        public LikelihoodRatioTestResults LikelihoodRatioTestResults { get; set; }
    }
}
