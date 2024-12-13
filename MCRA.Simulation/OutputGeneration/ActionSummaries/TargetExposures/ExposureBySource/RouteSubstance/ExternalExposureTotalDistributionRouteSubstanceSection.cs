using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureTotalDistributionRouteSubstanceSection : ExternalExposureDistributionRouteSubstanceSectionBase {

        public List<ExternalExposureDistributionRouteSubstanceRecord> Records { get; set; }

        public void Summarize(
            ICollection<Compound> selectedSubstances,
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposureRoutes = externalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key.GetExposureRoute())
                .Distinct()
                .ToList();

            Percentages = [lowerPercentage, 50, upperPercentage];
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(externalIndividualDayExposures, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
            } else {
                Records = SummarizeChronic(externalIndividualDayExposures, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }
        public void SummarizeUncertainty(
            ICollection<Compound> selectedSubstances,
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposureRoutes = externalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key.GetExposureRoute())
                .Distinct()
                .ToList();

            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(externalIndividualDayExposures, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(externalIndividualDayExposures, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);

            }
        }
        private void setUncertaintyBounds(
            List<ExternalExposureDistributionRouteSubstanceRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<ExternalExposureDistributionRouteSubstanceRecord> distributionCompoundRecords) {
            foreach (var record in Records) {
                var contribution = distributionCompoundRecords.FirstOrDefault(c => c.CompoundCode == record.CompoundCode && c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}