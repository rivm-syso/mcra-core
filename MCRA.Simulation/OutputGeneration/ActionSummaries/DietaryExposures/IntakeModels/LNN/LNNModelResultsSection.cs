using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class LNNModelResultsSection : SummarySection {
        public ErrorMessages Message { get; set; }
        public IntakeModelType FallBackModel { get; set; }
        public List<ModelFitResultSummaryRecord> FrequencyModelFitSummaryRecords { get; set; }
        public List<ModelFitResultSummaryRecord> AmountModelFitSummaryRecords { get; set; }

        public void Summarize(LNNModel lnnModel) {
            FallBackModel = lnnModel.FallBackModel;
            if (FallBackModel == IntakeModelType.LNN) {
                Message = lnnModel.FrequencyAmountModelSummary.ErrorMessage;

                FrequencyModelFitSummaryRecords = summarizeFrequencyModel(lnnModel.FrequencyAmountModelSummary);
                AmountModelFitSummaryRecords = summarizeAmountModel(lnnModel.FrequencyAmountModelSummary);
            }
        }

        private static List<ModelFitResultSummaryRecord> summarizeFrequencyModel(
            FrequencyAmountModelSummary frequencyAmountModelSummary
        ) {
            var varianceEstimates = frequencyAmountModelSummary.DispersionEstimates;
            var frequencyModelEstimates = frequencyAmountModelSummary.FrequencyModelEstimates;
            var degreesOfFreedom = frequencyAmountModelSummary.DegreesOfFreedomFrequencies;
            var _2LogLikelihood = frequencyAmountModelSummary._2LogLikelihood;

            var frequencyModelFitSummaryRecords = new List<ModelFitResultSummaryRecord>() {
                new() {
                    Parameter = varianceEstimates.ParameterName,
                    Estimate = varianceEstimates.Estimate,
                    StandardError = varianceEstimates.StandardError,
                }
            };
            foreach (var item in frequencyModelEstimates) {
                var record = new ModelFitResultSummaryRecord {
                    Parameter = item.ParameterName,
                    Estimate = item.Estimate,
                    StandardError = item.StandardError,
                };
                frequencyModelFitSummaryRecords.Add(record);
            }
            var dfRecord = new ModelFitResultSummaryRecord {
                Parameter = "degrees of freedom",
                Estimate = degreesOfFreedom
            };

            frequencyModelFitSummaryRecords.Add(dfRecord);

            var logLikRecord = new ModelFitResultSummaryRecord {
                Parameter = "-2*loglikelihood",
                Estimate = _2LogLikelihood
            };
            frequencyModelFitSummaryRecords.Add(logLikRecord);
            return frequencyModelFitSummaryRecords;
        }

        private static List<ModelFitResultSummaryRecord> summarizeAmountModel(
            FrequencyAmountModelSummary frequencyAmountModelSummary
        ) {
            var _2LogLikelihood = frequencyAmountModelSummary._2LogLikelihood;
            var power = frequencyAmountModelSummary.Power;
            var varianceBetween = frequencyAmountModelSummary.VarianceBetween;
            var varianceWithin = frequencyAmountModelSummary.VarianceWithin;
            var amountsModelEstimates = frequencyAmountModelSummary.AmountModelEstimates;
            var correlationEstimates = frequencyAmountModelSummary.CorrelationEstimates;
            var degreesOfFreedom = frequencyAmountModelSummary.DegreesOfFreedomAmounts;
            var amountModelFitSummaryRecords = new List<ModelFitResultSummaryRecord>() {
                new() {
                    Parameter = "transformation power",
                    Estimate = power
                }
            };
            foreach (var item in amountsModelEstimates) {
                var record = new ModelFitResultSummaryRecord {
                    Parameter = item.ParameterName,
                    Estimate = item.Estimate,
                    StandardError = item.StandardError,
                };
                amountModelFitSummaryRecords.Add(record);
            }
            var varBetweenRecord = new ModelFitResultSummaryRecord {
                Parameter = varianceBetween.ParameterName.ToLower(),
                Estimate = varianceBetween.Estimate,
                StandardError = varianceBetween.StandardError,
            };
            amountModelFitSummaryRecords.Add(varBetweenRecord);
            var varWithinRecord = new ModelFitResultSummaryRecord {
                Parameter = varianceWithin.ParameterName.ToLower(),
                Estimate = varianceWithin.Estimate,
                StandardError = varianceWithin.StandardError,
            };
            amountModelFitSummaryRecords.Add(varWithinRecord);
            var correlationRecord = new ModelFitResultSummaryRecord {
                Parameter = correlationEstimates.ParameterName.ToLower(),
                Estimate = correlationEstimates.Estimate,
                StandardError = correlationEstimates.StandardError,
            };
            amountModelFitSummaryRecords.Add(correlationRecord);
            var logLikRecord = new ModelFitResultSummaryRecord {
                Parameter = "-2*loglikelihood",
                Estimate = _2LogLikelihood
            };
            var dfRecord = new ModelFitResultSummaryRecord {
                Parameter = "degrees of freedom",
                Estimate = degreesOfFreedom
            };
            amountModelFitSummaryRecords.Add(dfRecord);

            amountModelFitSummaryRecords.Add(logLikRecord);
            return amountModelFitSummaryRecords;
        }
    }
}
