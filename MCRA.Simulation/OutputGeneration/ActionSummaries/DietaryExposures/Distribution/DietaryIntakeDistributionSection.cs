using MCRA.Utils;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DietaryIntakeDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        /// <summary>
        /// summarizes plots untransformed, total, upper, percentiles, percentages.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="exposureLevels"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="uncertaintyLowerLimit"></param>
        /// <param name="uncertaintyUpperLimit"></param>
        public void Summarize(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            double[] exposureLevels,
            double[] selectedPercentiles,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            var untransformedTotalIntakeDistributionSection = new UntransformedTotalIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(untransformedTotalIntakeDistributionSection, "Graph untransformed", 1);
            untransformedTotalIntakeDistributionSection.Summarize(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            subHeader.SaveSummarySection(untransformedTotalIntakeDistributionSection);

            var weights = dietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();
            var dietaryIntakes = dietaryIndividualDayIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();

            // Summarize total and upper distribution
            var coExposureIntakes = getCoExposures(dietaryIndividualDayIntakes);
            if (coExposureIntakes.Any()) {
                var totalIntakeDistributionSection = new DietaryTotalIntakeCoExposureDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(totalIntakeDistributionSection, "Graph total", 2);
                totalIntakeDistributionSection.Summarize(
                    coExposureIntakes,
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    GriddingFunctions.GetPlotPercentages(),
                    isPerPerson,
                    uncertaintyLowerLimit,
                    uncertaintyUpperLimit
                );
                subHeader.SaveSummarySection(totalIntakeDistributionSection);

                var intakeDistributionSection = new DietaryUpperIntakeCoExposureDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(intakeDistributionSection, "Graph upper tail", 3);
                intakeDistributionSection.Summarize(
                    coExposureIntakes,
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    percentageForUpperTail,
                    isPerPerson,
                    uncertaintyLowerLimit,
                    uncertaintyUpperLimit
                );
                subHeader.SaveSummarySection(intakeDistributionSection);
            } else {
                var totalIntakeDistributionSection = new DietaryTotalIntakeDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(totalIntakeDistributionSection, "Graph total", 2);
                totalIntakeDistributionSection.Summarize(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    GriddingFunctions.GetPlotPercentages(),
                    isPerPerson,
                    uncertaintyLowerLimit,
                    uncertaintyUpperLimit
                );
                subHeader.SaveSummarySection(totalIntakeDistributionSection);

                var upperIntakeDistributionSection = new DietaryUpperIntakeDistributionSection();
                subHeader = header.AddSubSectionHeaderFor(upperIntakeDistributionSection, "Graph upper tail", 3);
                upperIntakeDistributionSection.Summarize(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    percentageForUpperTail,
                    isPerPerson,
                    uncertaintyLowerLimit,
                    uncertaintyUpperLimit
                );
                subHeader.SaveSummarySection(upperIntakeDistributionSection);
            }

            // Summarize percentiles
            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 4);
            percentileSection.Summarize(dietaryIntakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            // Summarize percentages
            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 5);
            percentageSection.Summarize(dietaryIntakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes
        /// 1) Percentiles
        /// 2) Percentages
        /// 3) Percentiles for a grid of 100 values
        /// </summary>
        /// <param name="header"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var dietaryIntakes = dietaryIndividualDayIntakes
                .Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                .ToList();
            var weights = dietaryIndividualDayIntakes
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            // Summarize percentiles uncertainty
            var subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection?.SummarizeUncertainty(dietaryIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            // Summarize percentages uncertainty
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection?.SummarizeUncertainty(dietaryIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }

            // Percentiles for a grid of 100 values
            subHeader = header.GetSubSectionHeader<DietaryTotalIntakeDistributionSection>();
            if (subHeader != null) {
                var totalIntakeDistributionSection = subHeader.GetSummarySection() as DietaryTotalIntakeDistributionSection;
                totalIntakeDistributionSection?.SummarizeUncertainty(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
        }

        private HashSet<int> getCoExposures(ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes) {
            var cancelToken = ProgressState?.CancellationToken ?? new();
            return dietaryIndividualDayIntakes
                  .AsParallel()
                  .WithCancellation(cancelToken)
                  .Where(idi => idi.HasCoExposure())
                  .Select(c => c.SimulatedIndividualDayId)
                  .ToHashSet();
        }
    }
}
