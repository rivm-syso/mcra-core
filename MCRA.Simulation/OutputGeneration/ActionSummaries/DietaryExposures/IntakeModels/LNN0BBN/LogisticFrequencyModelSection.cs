using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class LogisticFrequencyModelSection : UncorrelatedModelResultsSection {

        public List<ModelFitResultSummaryRecord> FrequencyModelRecords { get; set; }

        public void Summarize(LNN0Model lnn0Model) {
            DegreesOfFreedom = lnn0Model.FrequencyModelSummary.DegreesOfFreedom;
            FrequencyModelEstimates = lnn0Model.FrequencyModelSummary.FrequencyModelEstimates;
            _2LogLikelihood = lnn0Model.FrequencyModelSummary._2LogLikelihood;
            VarianceEstimates = lnn0Model.FrequencyModelSummary.DispersionEstimates;
            LikelihoodRatioTestResults = lnn0Model.FrequencyModelSummary.LikelihoodRatioTestResults;
            Message = lnn0Model.FrequencyModelSummary.ErrorMessage;
            FrequencyModelRecords = getFrequencyRecords();
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
            FrequencyModelRecords = getFrequencyRecords();
        }

        private List<ModelFitResultSummaryRecord> getFrequencyRecords() {
            FrequencyModelRecords = [
                new ModelFitResultSummaryRecord {
                    Parameter = VarianceEstimates.ParameterName,
                    Estimate = VarianceEstimates.Estimate,
                    StandardError = VarianceEstimates.StandardError,
                }];

            foreach (var item in FrequencyModelEstimates) {
                var record = new ModelFitResultSummaryRecord {
                    Parameter = item.ParameterName,
                    Estimate = item.Estimate,
                    StandardError = item.StandardError,
                };
                FrequencyModelRecords.Add(record);
            }
            var dfRecord = new ModelFitResultSummaryRecord {
                Parameter = "degrees of freedom",
                Estimate = DegreesOfFreedom
            };
            FrequencyModelRecords.Add(dfRecord);
            var logLikRecord = new ModelFitResultSummaryRecord {
                Parameter = "-2*loglikelihood",
                Estimate = _2LogLikelihood
            };
            FrequencyModelRecords.Add(logLikRecord);
            return FrequencyModelRecords;
        }
    }
}