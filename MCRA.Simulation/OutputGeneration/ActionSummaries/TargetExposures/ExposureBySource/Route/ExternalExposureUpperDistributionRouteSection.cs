using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureUpperDistributionRouteSection : ExternalExposureDistributionRouteSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public List<ExternalExposureDistributionRouteRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="externalExposureCollection"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="exposureType"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        ///
        public void Summarize(
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
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
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new ExternalExposureUpperExposuresCalculator();
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                   externalIndividualDayExposures,
                   relativePotencyFactors,
                   membershipProbabilities,
                   exposureType,
                   percentageForUpperTail,
                   isPerPerson
                );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(upperIntakes, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var nonDietaryUpperIntakes = upperIntakes
                        .Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = nonDietaryUpperIntakes.Min();
                    HighPercentileValue = nonDietaryUpperIntakes.Max();
                }
            } else {
                Records = SummarizeChronic(upperIntakes, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Select(c => c.SimulatedIndividualId).Distinct().Count();
                if (NRecords > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => c.Average(i => i.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / externalIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureType"></param>
        /// <param name="isPerPerson"></param>
        ///
        public void SummarizeUncertainty(
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposureRoutes = externalExposureCollection.ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key.GetExposureRoute())
                .Distinct()
                .ToList();

            var upperIntakeCalculator = new ExternalExposureUpperExposuresCalculator();
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, exposureType, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(upperIntakes, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);

            } else {
                var records = SummarizeChronicUncertainty(upperIntakes, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
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
