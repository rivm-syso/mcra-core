using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryTotalDistributionRouteSection : NonDietaryDistributionRouteSectionBase {

        public List<NonDietaryDistributionRouteRecord> NonDietaryTotalDistributionRouteRecords { get; set; }

        public void Summarize(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRouteType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            if (exposureType == ExposureType.Acute) {
                NonDietaryTotalDistributionRouteRecords = SummarizeAcute(
                    nonDietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    nonDietaryExposureRoutes,
                    isPerPerson
                );
            } else {
                NonDietaryTotalDistributionRouteRecords = SummarizeChronic(
                    nonDietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    nonDietaryExposureRoutes,
                    isPerPerson
                );
            }
            setUncertaintyBounds(NonDietaryTotalDistributionRouteRecords, uncertaintyLowerBound, uncertaintyUpperBound);
        }
        public void SummarizeUncertainty(
           ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRouteType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            bool isPerPerson
       ) {
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);

            }
        }
        private void setUncertaintyBounds(
            List<NonDietaryDistributionRouteRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<NonDietaryDistributionRouteRecord> records) {
            foreach (var record in NonDietaryTotalDistributionRouteRecords) {
                var contribution = records.Where(c => c.ExposureRoute == record.ExposureRoute).FirstOrDefault()?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
