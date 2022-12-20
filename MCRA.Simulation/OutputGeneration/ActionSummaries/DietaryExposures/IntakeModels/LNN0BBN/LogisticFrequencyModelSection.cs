using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class LogisticFrequencyModelSection : UncorrelatedModelResultsSection {

        public void Summarize(LNN0Model lnn0Model) {
            DegreesOfFreedom = lnn0Model.FrequencyModelSummary.DegreesOfFreedom;
            FrequencyModelEstimates = lnn0Model.FrequencyModelSummary.FrequencyModelEstimates;
            _2LogLikelihood = lnn0Model.FrequencyModelSummary._2LogLikelihood;
            VarianceEstimates = lnn0Model.FrequencyModelSummary.DispersionEstimates;
            LikelihoodRatioTestResults = lnn0Model.FrequencyModelSummary.LikelihoodRatioTestResults;
            Message = lnn0Model.FrequencyModelSummary.ErrorMessage;
        }

        /// <summary>
        /// Only for LNN initial estimates
        /// </summary>
        /// <param name="frequencyModelSummary"></param>
        public void Summarize(FrequencyModelSummary frequencyModelSummary) {
            DegreesOfFreedom = frequencyModelSummary.DegreesOfFreedom;
            FrequencyModelEstimates = frequencyModelSummary.FrequencyModelEstimates;
            _2LogLikelihood = frequencyModelSummary._2LogLikelihood;
            VarianceEstimates = frequencyModelSummary.DispersionEstimates;
            LikelihoodRatioTestResults = frequencyModelSummary.LikelihoodRatioTestResults;
            Message = frequencyModelSummary.ErrorMessage;
        }
    }
}
