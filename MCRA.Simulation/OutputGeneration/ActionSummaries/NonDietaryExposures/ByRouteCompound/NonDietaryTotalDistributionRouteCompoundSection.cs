using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryTotalDistributionRouteCompoundSection : NonDietaryDistributionRouteCompoundSectionBase {

        public List<NonDietaryDistributionRouteCompoundRecord> NonDietaryTotalDistributionRouteCompoundRecords { get; set; }

        public void Summarize(
            ICollection<Compound> selectedSubstances,
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
                NonDietaryTotalDistributionRouteCompoundRecords = SummarizeAcute(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
            } else {
                NonDietaryTotalDistributionRouteCompoundRecords = SummarizeChronic(nonDietaryIndividualDayIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
            }
            setUncertaintyBounds(NonDietaryTotalDistributionRouteCompoundRecords, uncertaintyLowerBound, uncertaintyUpperBound);
        }
        public void SummarizeUncertainty(
            ICollection<Compound> selectedSubstances,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRouteType> nonDietaryExposureRoutes,
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
            foreach (var record in NonDietaryTotalDistributionRouteCompoundRecords) {
                var contribution = distributionCompoundRecords.Where(c => c.CompoundCode == record.CompoundCode && c.ExposureRoute == record.ExposureRoute).FirstOrDefault()?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}