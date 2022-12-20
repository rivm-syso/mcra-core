using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BetaBinomialFrequencyModelSection : UncorrelatedModelResultsSection {

        public void Summarize(BBNModel bbnModel) {
            DegreesOfFreedom = bbnModel.FrequencyModelSummary.DegreesOfFreedom;
            FrequencyModelEstimates = bbnModel.FrequencyModelSummary.FrequencyModelEstimates;
            _2LogLikelihood = bbnModel.FrequencyModelSummary._2LogLikelihood;
            DispersionEstimates = bbnModel.FrequencyModelSummary.DispersionEstimates;
            LikelihoodRatioTestResults = bbnModel.FrequencyModelSummary.LikelihoodRatioTestResults;
            Message = bbnModel.FrequencyModelSummary.ErrorMessage;
        }
    }
}
