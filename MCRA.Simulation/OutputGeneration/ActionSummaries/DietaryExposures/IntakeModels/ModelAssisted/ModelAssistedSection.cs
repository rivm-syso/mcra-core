using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ModelAssistedSection : ChronicSectionBase {

        /// <summary>
        /// Summarizes dietary individual intakes chronic model assisted for BBN, LNN0, LNN.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryModelAssistedIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="percentageForUpperTail"></param>
        public void Summarize(
            SectionHeader header,
            List<DietaryIndividualIntake> dietaryModelAssistedIntakes,
            Compound referenceSubstance,
            ExposureMethod exposureMethod,
            double[] selectedPercentiles,
            double[] selectedExposureLevels,
            double percentageForUpperTail
        ) {
            var intakes = dietaryModelAssistedIntakes.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = dietaryModelAssistedIntakes.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );
            var totalDistributionSection = new ModelAssistedDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 10);
            totalDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(totalDistributionSection);

            var upperDistributionSection = new ModelAssistedDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 11);
            upperDistributionSection.SummarizeUpperDietary(dietaryModelAssistedIntakes, percentageForUpperTail);
            subHeader.SaveSummarySection(upperDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 20);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 30);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes the uncertainty results model assisted.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="modelAssistedIntakes"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                SectionHeader header,
                List<DietaryIndividualIntake> modelAssistedIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var usualIntakes = modelAssistedIntakes.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = modelAssistedIntakes.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();

            var subHeader = header.GetSubSectionHeader<ModelAssistedDistributionSection>();
            if (subHeader != null) {
                var intakeDistributionSection = subHeader.GetSummarySection() as ModelAssistedDistributionSection;
                intakeDistributionSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(usualIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}
