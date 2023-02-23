using MCRA.Utils;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryIntakeDistributionSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public void Summarize(
            SectionHeader header,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            Compound referenceSubstance,
            double[] selectedPercentiles,
            double[] exposureLevels,
            double percentageForUpperTail,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            var coExposureIntakes = getCoExposures(nonDietaryDayIntakes);
            var totalIntakeDistributionSection = new NonDietaryTotalIntakeDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(totalIntakeDistributionSection, "Graph total", 1);
            totalIntakeDistributionSection.Summarize(
                coExposureIntakes,
                nonDietaryDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                GriddingFunctions.GetPlotPercentages(),
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
            );
            subHeader.SaveSummarySection(totalIntakeDistributionSection);

            var upperIntakeDistributionSection = new NonDietaryUpperIntakeDistributionSection();
            subHeader = header.AddSubSectionHeaderFor(upperIntakeDistributionSection, "Graph upper tail", 2);
            upperIntakeDistributionSection.Summarize(
                coExposureIntakes,
                nonDietaryDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                percentageForUpperTail,
                isPerPerson,
                uncertaintyLowerLimit,
                uncertaintyUpperLimit
           );
            subHeader.SaveSummarySection(upperIntakeDistributionSection);

            var nonDietaryIntakes = nonDietaryDayIntakes.Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = nonDietaryDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 3);
            percentileSection.Summarize(nonDietaryIntakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 4);
            percentageSection.Summarize(nonDietaryIntakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes
        /// 1) Percentiles
        /// 2) Percentages
        /// 3) Percentiles for a grid of 100 values
        /// </summary>
        /// <param name="header"></param>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        public void SummarizeUncertainty(
            SectionHeader header,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var nonDietaryIntakes = nonDietaryIndividualDayIntakes.Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).ToList();
            var weights = nonDietaryIndividualDayIntakes.Select(c => c.IndividualSamplingWeight).ToList();
            var subHeader = header.GetSubSectionHeader<NonDietaryTotalIntakeDistributionSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as NonDietaryTotalIntakeDistributionSection;
                section.SummarizeUncertainty(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentileSection;
                section.SummarizeUncertainty(nonDietaryIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IntakePercentageSection;
                section.SummarizeUncertainty(nonDietaryIntakes, weights, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }

        private List<int> getCoExposures(ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes) {
            var cancelToken = ProgressState?.CancellationToken ?? new System.Threading.CancellationToken();
            return nonDietaryIndividualDayIntakes
                .AsParallel()
                .WithCancellation(cancelToken)
                .Select(idi => (
                    SimulatedIndividualDayId: idi.SimulatedIndividualDayId,
                    IntakesPerCompound: idi.GetTotalIntakesPerCompound().Count(g => g.Exposure > 0)
                ))
                .Where(ipc => ipc.IntakesPerCompound > 1)
                .Select(c => c.SimulatedIndividualDayId)
                .ToList();
        }
    }
}
