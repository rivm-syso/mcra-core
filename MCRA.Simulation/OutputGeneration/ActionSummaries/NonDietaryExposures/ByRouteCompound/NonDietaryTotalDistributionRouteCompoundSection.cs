using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryTotalDistributionRouteCompoundSection : NonDietaryDistributionRouteCompoundSectionBase {

        public List<NonDietaryDistributionRouteCompoundRecord> Records { get; set; }

        public void Summarize(
            ICollection<Compound> selectedSubstances,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
            } else {
                Records = SummarizeChronic(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }
        public void SummarizeUncertainty(
            ICollection<Compound> selectedSubstances,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> nonDietaryExposureRoutes,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);

            }
        }
        private void setUncertaintyBounds(
            List<NonDietaryDistributionRouteCompoundRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<NonDietaryDistributionRouteCompoundRecord> distributionCompoundRecords) {
            foreach (var record in Records) {
                var contribution = distributionCompoundRecords.FirstOrDefault(c => c.CompoundCode == record.CompoundCode && c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}