using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConditionalIntakePercentileSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<CovariatesCollectionIntakePercentileSection> ConditionalIntakePercentileSections;

        /// <summary>
        /// Summarizes the exposures for OIM. Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="cofactor"></param>
        /// <param name="covariable"></param>
        /// <param name="usualIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="selectedPercentiles"></param>
        public void Summarize(
            IndividualProperty cofactor,
            IndividualProperty covariable,
            Compound referenceSubstance,
            List<ConditionalUsualIntake> usualIntakes,
            double[] selectedPercentiles
        ) {
            ConditionalIntakePercentileSections = new List<CovariatesCollectionIntakePercentileSection>();
            foreach (var item in usualIntakes) {
                var intakePercentileSection = new IntakePercentileSection();
                var covariatesCollection = new CovariatesCollection() {
                    AmountCofactor = item.CovariatesCollection.AmountCofactor,
                    AmountCovariable = item.CovariatesCollection.AmountCovariable,
                    FrequencyCofactor = item.CovariatesCollection.FrequencyCofactor,
                    FrequencyCovariable = item.CovariatesCollection.FrequencyCovariable,
                    CofactorName = cofactor?.Name,
                    CovariableName = covariable?.Name
                };
                intakePercentileSection.Summarize(
                    item.ConditionalUsualIntakes,
                    null,
                    referenceSubstance,
                    selectedPercentiles
                );
                ConditionalIntakePercentileSections.Add(new CovariatesCollectionIntakePercentileSection() {
                    CovariatesCollection = covariatesCollection,
                    IntakePercentileSection = intakePercentileSection,
                });
            }
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for OIM. Percentiles (output)
        /// from specified percentages (input).
        /// </summary>
        /// <param name="usualIntakes"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                List<ConditionalUsualIntake> usualIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            foreach (var item in ConditionalIntakePercentileSections) {
                var cuic = usualIntakes.FirstOrDefault(ui => ui.CovariatesCollection.AmountCofactor==item.CovariatesCollection.AmountCofactor
                            && ui.CovariatesCollection.AmountCovariable.Equals(item.CovariatesCollection.AmountCovariable)
                            && ui.CovariatesCollection.FrequencyCofactor==item.CovariatesCollection.FrequencyCofactor
                            && ui.CovariatesCollection.FrequencyCovariable.Equals(item.CovariatesCollection.FrequencyCovariable));
                if (cuic != null) {
                    item.IntakePercentileSection.SummarizeUncertainty(cuic.ConditionalUsualIntakes, null, uncertaintyLowerBound, uncertaintyUpperBound);
                }
            }
        }
    }
}
