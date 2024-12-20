using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureUpperDistributionRouteSubstanceSection : ExternalExposureDistributionRouteSubstanceSectionBase {

        public List<ExternalExposureDistributionRouteSubstanceRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<Compound> selectedSubstances,
            ExternalExposureCollection externalExposureCollection,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
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
            var upperIntakes = upperIntakeCalculator.GetUpperExposures(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    isPerPerson
            );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var externalExposureUpperIntakes = upperIntakes
                        .Select(c => c.GetTotalExternalExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = externalExposureUpperIntakes.Min();
                    HighPercentileValue = externalExposureUpperIntakes.Max();
                }
            } else {
                //Upper deugt nog niet moet op basis van individuals (niet days)
                Records = SummarizeChronic(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
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

        public void SummarizeUncertainty(
            ICollection<Compound> selectedSubstances,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new ExternalExposureUpperExposuresCalculator();
            var upperIntakes = upperIntakeCalculator.GetUpperExposures(
                    externalIndividualDayExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    isPerPerson
            );
            
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, externalExposureRoutes, isPerPerson);
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

        private void updateContributions(List<ExternalExposureDistributionRouteSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.SubstanceCode == record.SubstanceCode && c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
