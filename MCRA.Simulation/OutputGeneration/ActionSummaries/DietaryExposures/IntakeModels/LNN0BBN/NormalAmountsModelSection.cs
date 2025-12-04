using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NormalAmountsModelSection : UncorrelatedModelResultsSection {
        public double Power { get; set; }
        public double VarianceBetween { get; set; }
        public double VarianceWithin { get; set; }
        public List<ParameterEstimates> AmountsModelEstimates { get; set; }

        public List<ModelFitResultSummaryRecord> AmountModelFitSummaryRecords;

        public void Summarize(AmountsModelSummary amountsModelSummary, bool isAcuteCovariateModelling = false) {
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

            AmountModelFitSummaryRecords = [
                new ModelFitResultSummaryRecord {
                    Parameter = "transformation power",
                    Estimate = Power
                }
            ];
            foreach (var item in AmountsModelEstimates) {
                var record = new ModelFitResultSummaryRecord {
                    Parameter = item.ParameterName,
                    Estimate = item.Estimate,
                    StandardError = item.StandardError,
                    TValue = item.TValue
                };
                AmountModelFitSummaryRecords.Add(record);
            }
            if (isAcuteCovariateModelling) {
                var varBetweenRecord = new ModelFitResultSummaryRecord {
                    Parameter = "distribution variance",
                    Estimate = VarianceBetween
                };
                AmountModelFitSummaryRecords.Add(varBetweenRecord);
            } else {
                var varBetweenRecord = new ModelFitResultSummaryRecord {
                    Parameter = "variance between individuals",
                    Estimate = VarianceBetween
                };
                AmountModelFitSummaryRecords.Add(varBetweenRecord);
                var varWithinRecord = new ModelFitResultSummaryRecord {
                    Parameter = "variance within individuals",
                    Estimate = VarianceWithin
                };
                AmountModelFitSummaryRecords.Add(varWithinRecord);
            }
            var DfRecord = new ModelFitResultSummaryRecord {
                Parameter = "degrees of freemdom",
                Estimate = DegreesOfFreedom
            };
            AmountModelFitSummaryRecords.Add(DfRecord);
            var LogLikRecord = new ModelFitResultSummaryRecord {
                Parameter = "-2*loglikelihood",
                Estimate = _2LogLikelihood
            };
            AmountModelFitSummaryRecords.Add(LogLikRecord);
        }
    }
}
