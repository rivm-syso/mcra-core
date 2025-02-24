using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureLevelsCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicNonDietarySection : ChronicSectionBase {

        /// <summary>
        /// Summarizes  nondietary exposures
        /// </summary>
        /// <param name="header"></param>
        /// <param name="nonDietaryIndividualMeans"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="exposureMethod"></param>
        /// <param name="selectedExposureLevels"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="isPerPerson"></param>
        public void Summarize(
            SectionHeader header,
            List<NonDietaryIndividualIntake> nonDietaryIndividualMeans,
            Compound referenceSubstance,
            double upperPercentage,
            ExposureMethod exposureMethod,
            double[] selectedExposureLevels,
            double[] selectedPercentiles,
            bool isPerPerson
        ) {
            var intakes = nonDietaryIndividualMeans.Select(c => c.NonDietaryIntakePerBodyWeight * (isPerPerson ? c.Individual.BodyWeight : 1)).ToList();
            var weights = nonDietaryIndividualMeans.Select(c => c.IndividualSamplingWeight).ToList();
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
            upperDistributionSection.SummarizeUpperNonDietary(nonDietaryIndividualMeans, upperPercentage, isPerPerson);
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

        public void SummarizeUncertainty(
                SectionHeader header,
                List<NonDietaryIndividualIntake> nonDietaryIndividualIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var intakes = nonDietaryIndividualIntakes.Select(c => c.NonDietaryIntakePerBodyWeight).ToList();
            var weights = nonDietaryIndividualIntakes.Select(c => c.IndividualSamplingWeight).ToList();
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
