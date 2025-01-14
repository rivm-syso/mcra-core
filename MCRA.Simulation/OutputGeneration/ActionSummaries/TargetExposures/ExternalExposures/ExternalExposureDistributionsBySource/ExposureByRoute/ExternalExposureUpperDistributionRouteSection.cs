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

        public void Summarize(
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            ExposureType exposureType,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            if (relativePotencyFactors == null && substances.Count == 1) {
                relativePotencyFactors = substances.ToDictionary(r => r, r => 1D);
            }
            if (membershipProbabilities == null && substances.Count == 1) {
                membershipProbabilities = substances.ToDictionary(r => r, r => 1D);
            }

            var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
            var externalExposureRoutes = externalExposureCollection.ExternalIndividualDayExposures
                .SelectMany(r => r.ExposuresPerRouteSubstance)
                .Select(r => r.Key)
                .Distinct()
                .ToList();

            Percentages = [lowerPercentage, 50, upperPercentage];
            UpperPercentage = 100 - percentageForUpperTail;
            var upperExposureCalculator = new ExternalExposureUpperExposuresCalculator();
            var upperExposures = upperExposureCalculator
                .GetUpperExposures(
                   externalIndividualDayExposures,
                   relativePotencyFactors,
                   membershipProbabilities,
                   exposureType,
                   percentageForUpperTail,
                   isPerPerson
                );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(upperExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                NRecords = upperExposures.Count;
                if (NRecords > 0) {
                    var externalUpperExposures = upperExposures
                        .Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = externalUpperExposures.Min();
                    HighPercentileValue = externalUpperExposures.Max();
                }
            } else {
                Records = SummarizeChronic(upperExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                NRecords = upperExposures.Select(c => c.SimulatedIndividual.Id).Distinct().Count();
                if (NRecords > 0) {
                    var oims = upperExposures
                        .GroupBy(c => c.SimulatedIndividual.Id)
                        .Select(c => c.Average(i => i.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }
            CalculatedUpperPercentage = upperExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) / externalIndividualDayExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        /// <param name="externalExposureCollection"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
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
                .Select(r => r.Key)
                .Distinct()
                .ToList();

            var upperExposureCalculator = new ExternalExposureUpperExposuresCalculator();
            var upperExposures = upperExposureCalculator.GetUpperExposures(externalIndividualDayExposures, relativePotencyFactors, membershipProbabilities, exposureType, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(upperExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);

            } else {
                var records = SummarizeChronicUncertainty(upperExposures, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
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
