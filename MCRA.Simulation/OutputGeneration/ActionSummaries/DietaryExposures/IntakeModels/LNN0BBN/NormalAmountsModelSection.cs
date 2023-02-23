using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NormalAmountsModelSection : UncorrelatedModelResultsSection {
        public bool IsAcuteCovariateModelling { get; set; }
        public double Power { get; set; }
        public double VarianceBetween { get; set; }
        public double VarianceWithin { get; set; }
        public List<ParameterEstimates> AmountsModelEstimates { get; set; }

        public void Summarize(AmountsModelSummary amountsModelSummary, bool isAcuteCovariateModelling) {
            this.IsAcuteCovariateModelling = isAcuteCovariateModelling;
            this.VarianceBetween = ((NormalAmountsModelSummary)amountsModelSummary).VarianceBetween;
            this.VarianceWithin = ((NormalAmountsModelSummary)amountsModelSummary).VarianceWithin;
            this._2LogLikelihood = ((NormalAmountsModelSummary)amountsModelSummary)._2LogLikelihood;
            this.AmountsModelEstimates = ((NormalAmountsModelSummary)amountsModelSummary).AmountModelEstimates;
            this.DegreesOfFreedom = ((NormalAmountsModelSummary)amountsModelSummary).DegreesOfFreedom;
            if (((NormalAmountsModelSummary)amountsModelSummary).IntakeTransformer is PowerTransformer) {
                Power = (((NormalAmountsModelSummary)amountsModelSummary).IntakeTransformer as PowerTransformer).Power;
            }
            if (((NormalAmountsModelSummary)amountsModelSummary).IntakeTransformer is IdentityTransformer) {
                Power = 1;
            }
            if (((NormalAmountsModelSummary)amountsModelSummary).IntakeTransformer is LogTransformer) {
                Power = 0;
            }
            this.LikelihoodRatioTestResults = ((NormalAmountsModelSummary)amountsModelSummary).LikelihoodRatioTestResults;
        }

        /// <summary>
        /// Overload for LNN with correlation.
        /// </summary>
        /// <param name="amountsModelSummary"></param>
        public void Summarize(AmountsModelSummary amountsModelSummary) {
            Summarize(amountsModelSummary, false);
        }
    }
}
