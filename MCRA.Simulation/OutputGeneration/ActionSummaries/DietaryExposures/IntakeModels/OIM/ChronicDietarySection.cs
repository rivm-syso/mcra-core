using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Observed individual means
    /// </summary>
    public class ChronicDietarySection : ChronicSectionBase {

        /// <summary>
        /// Summarize observed individual mean
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryObservedIndividualMeans"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            List<DietaryIndividualIntake> dietaryObservedIndividualMeans,
            ExposureMethod exposureMethod,
            Compound referenceSubstance,
            double[] selectedExposureLevels,
            double[] selectedPercentiles,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var intakes = dietaryObservedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = dietaryObservedIndividualMeans.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var exposureLevels = ExposureLevelsCalculator.GetExposureLevels(
                intakes,
                exposureMethod,
                selectedExposureLevels
            );

            var totalDistributionSection = new OIMDistributionSection(true);
            var subHeader = header.AddSubSectionHeaderFor(totalDistributionSection, "Graph total", 1);
            totalDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(totalDistributionSection);

            var upperDistributionSection = new OIMDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperDistributionSection, "Graph upper tail", 2);
            upperDistributionSection.SummarizeUpperDietary(dietaryObservedIndividualMeans, percentageForUpperTail);
            subHeader.SaveSummarySection(upperDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryObservedIndividualMeans"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            List<DietaryIndividualIntake> dietaryObservedIndividualMeans,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            var intakes = dietaryObservedIndividualMeans.Select(c => c.DietaryIntakePerMassUnit).ToList();
            var weights = dietaryObservedIndividualMeans.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var subHeader = header.GetSubSectionHeader<OIMDistributionSection>();
            if (subHeader != null) {
                var totalDistributionSection = subHeader.GetSummarySection() as OIMDistributionSection;
                totalDistributionSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(intakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}
