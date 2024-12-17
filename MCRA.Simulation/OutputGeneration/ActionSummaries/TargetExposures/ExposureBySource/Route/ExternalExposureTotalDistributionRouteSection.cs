using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureTotalDistributionRouteSection : ExternalExposureDistributionRouteSectionBase {

        public List<ExternalExposureDistributionRouteRecord> Records { get; set; }

        public void Summarize(
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
            var externalExposureRoutes = externalExposureCollection.ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key.GetExposureRoute())
                .Distinct()
                .ToList();

            Percentages = [lowerPercentage, 50, upperPercentage];
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    externalExposureRoutes,
                    isPerPerson
                );
            } else {
                Records = SummarizeChronic(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    externalExposureRoutes,
                    isPerPerson
                );
            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }
        public void SummarizeUncertainty(
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            bool isPerPerson
       ) {
            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposureRoutes = externalExposureCollection.ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key.GetExposureRoute())
                .Distinct()
                .ToList();

            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);
            }
        }

        private void setUncertaintyBounds(
            List<ExternalExposureDistributionRouteRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<ExternalExposureDistributionRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
