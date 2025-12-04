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
        }

        /// <summary>
        /// Only for LNN initial estimates
        /// </summary>
        /// <param name="frequencyModelSummary"></param>
        public void Summarize(FrequencyModelSummary frequencyModelSummary) {
            DegreesOfFreedom = frequencyModelSummary.DegreesOfFreedom;
            FrequencyModelEstimates = frequencyModelSummary.FrequencyModelEstimates;
            VarianceEstimates = frequencyModelSummary.DispersionEstimates;
            LikelihoodRatioTestResults = frequencyModelSummary.LikelihoodRatioTestResults;
            Message = frequencyModelSummary.ErrorMessage;

            var varianceEstimates = frequencyModelSummary.DispersionEstimates;
            var frequencyModelEstimates = frequencyModelSummary.FrequencyModelEstimates;
            var degreesOfFreedom = frequencyModelSummary.DegreesOfFreedom;
            var _2LogLikelihood = frequencyModelSummary._2LogLikelihood;

            FrequencyModelRecords = [
                new ModelFitResultSummaryRecord {
                    Parameter = varianceEstimates.ParameterName,
                    Estimate = varianceEstimates.Estimate,
                    StandardError = varianceEstimates.StandardError,
                    TValue = varianceEstimates.TValue
                }
            ];
            foreach (var item in frequencyModelEstimates) {
                var record = new ModelFitResultSummaryRecord {
                    Parameter = item.ParameterName,
                    Estimate = item.Estimate,
                    StandardError = item.StandardError,
                    TValue = item.TValue
                };
                FrequencyModelRecords.Add(record);
            }
            var dfRecord = new ModelFitResultSummaryRecord {
                Parameter = "degrees of freedom",
                Estimate = degreesOfFreedom
            };
            FrequencyModelRecords.Add(dfRecord);
            var logLikRecord = new ModelFitResultSummaryRecord {
                Parameter = "-2*loglikelihood",
                Estimate = _2LogLikelihood
            };
            FrequencyModelRecords.Add(logLikRecord);
        }
    }
}
