using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConditionalIntakePercentageSection : ChronicSectionBase {

        public override bool SaveTemporaryData => true;

        public List<CovariatesCollectionIntakePercentageSection> ConditionalIntakePercentageSections;

        /// <summary>
        /// Summarizes the conditional intakes for BBN/LNN0. Percentiles (output) from specified percentiles (input).
        /// </summary>
        /// <param name="cofactor"></param>
        /// <param name="covariable"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="usualIntakes"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedExposureLevels"></param>
        public void Summarize(
                IndividualProperty cofactor,
                IndividualProperty covariable,
                Compound referenceSubstance,
                List<ConditionalUsualIntake> usualIntakes,
                ExposureMethod exposureMethod,
                double[] selectedExposureLevels
            ) {
            ConditionalIntakePercentageSections = [];
            foreach (var item in usualIntakes) {
                var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                    item.ConditionalUsualIntakes,
                    exposureMethod,
                    selectedExposureLevels
                );
                var covariatesCollection = new CovariatesCollection() {
                    AmountCofactor = item.CovariatesCollection.AmountCofactor,
                    AmountCovariable = item.CovariatesCollection.AmountCovariable,
                    FrequencyCofactor = item.CovariatesCollection.FrequencyCofactor,
                    FrequencyCovariable = item.CovariatesCollection.FrequencyCovariable,
                    CofactorName = cofactor?.Name,
                    CovariableName = covariable?.Name
                };
                var intakePercentageSection = new IntakePercentageSection();
                intakePercentageSection.Summarize(item.ConditionalUsualIntakes, null, referenceSubstance, exposureLevels);
                ConditionalIntakePercentageSections.Add(new CovariatesCollectionIntakePercentageSection() {
                    CovariatesCollection = covariatesCollection,
                    IntakePercentageSection = intakePercentageSection,
                });
            }
        }

        /// <summary>
        /// Summarizes the exposures of a bootstrap cycle for BBN/LNN0. Make sure
        /// to call SummarizeReferenceResults first.  Percentiles (output) from
        /// specified percentages (input).
        /// </summary>
        /// <param name="usualIntakes"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
            List<ConditionalUsualIntake> usualIntakes,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in ConditionalIntakePercentageSections) {
                var cuic = usualIntakes.FirstOrDefault(ui => ui.CovariatesCollection.AmountCofactor == item.CovariatesCollection.AmountCofactor
                            && ui.CovariatesCollection.AmountCovariable.Equals(item.CovariatesCollection.AmountCovariable)
                            && ui.CovariatesCollection.FrequencyCofactor == item.CovariatesCollection.FrequencyCofactor
                            && ui.CovariatesCollection.FrequencyCovariable.Equals(item.CovariatesCollection.FrequencyCovariable));
                if (cuic != null) {
                    item.IntakePercentageSection.SummarizeUncertainty(cuic.ConditionalUsualIntakes, null, uncertaintyLowerBound, uncertaintyUpperBound);
                }
            }
        }
    }
}
