using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExternalExposureUpperDistributionRouteSubstanceSection : ExternalExposureDistributionRouteSubstanceSectionBase {

        public List<ExternalExposureDistributionRouteSubstanceRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double? UpperPercentage { get; set; } = null;
        public double CalculatedUpperPercentage { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<Compound> substances,
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
            if (substances.Count == 1 || relativePotencyFactors != null) {
                var externalIndividualDayExposures = externalExposureCollection.ExternalIndividualDayExposures;
                var externalExposureRoutes = externalExposureCollection.ExternalIndividualDayExposures
                    .SelectMany(r => r.ExposuresPerPath)
                    .Select(r => r.Key.Route)
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
                    Records = SummarizeAcute(
                        upperIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        externalExposureRoutes,
                        isPerPerson
                    );
                    NumberOfIntakes = upperIntakes.Count;
                    if (NumberOfIntakes > 0) {
                        var externalExposureUpperIntakes = upperIntakes
                            .Select(c => c.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson))
                            .ToList();
                        LowPercentileValue = externalExposureUpperIntakes.Min();
                        HighPercentileValue = externalExposureUpperIntakes.Max();
                    }
                } else {
                    //Upper deugt nog niet moet op basis van individuals (niet days)
                    //Calculations are ok because a GroupBy over SimulatedIndividualIds is taken
                    Records = SummarizeChronic(
                        upperIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        externalExposureRoutes,
                        isPerPerson
                    );
                    NumberOfIntakes = upperIntakes.Select(c => c.SimulatedIndividual.Id).Distinct().Count();
                    if (NumberOfIntakes > 0) {
                        var oims = upperIntakes
                            .GroupBy(c => c.SimulatedIndividual.Id)
                            .Select(c => c.Average(i => i.GetExposure(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                            .ToList();
                        LowPercentileValue = oims.Min();
                        HighPercentileValue = oims.Max();
                    }
                }
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / externalIndividualDayExposures.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
                setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }

        public void SummarizeUncertainty(
            ICollection<Compound> substances,
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposureRoute> externalExposureRoutes,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            if (substances.Count == 1 || relativePotencyFactors != null) {
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
                    var records = SummarizeAcuteUncertainty(
                        upperIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        externalExposureRoutes,
                        isPerPerson
                    );
                    updateContributions(records);
                } else {
                    var records = SummarizeChronicUncertainty(
                        upperIntakes,
                        substances,
                        relativePotencyFactors,
                        membershipProbabilities,
                        externalExposureRoutes,
                        isPerPerson
                    );
                    updateContributions(records);
                }
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
