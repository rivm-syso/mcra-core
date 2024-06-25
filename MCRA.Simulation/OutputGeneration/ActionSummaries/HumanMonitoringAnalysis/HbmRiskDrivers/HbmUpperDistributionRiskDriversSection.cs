using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmUpperDistributionRiskDriversSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<HbmRiskDriverRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        /// <summary>
        /// Summarize risk drivers
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <param name="hbmIndividualCollections"></param>
        /// <param name="hbmCumulativeIndividualDayCollection"></param>
        /// <param name="hbmCumulativeIndividualCollection"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="exposureType"></param>
        /// <param name="percentageForUpperTail"></param>
        public void Summarize(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType,
            double percentageForUpperTail
        ) {
            if (exposureType == ExposureType.Acute) {
                computeAcuteRiskDrivers(
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    percentageForUpperTail
                );
            } else {
                computeChronicRiskDrivers(
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    percentageForUpperTail
                );
            }
        }

        /// <summary>
        /// Summarize risk drivers uncertainty
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <param name="hbmIndividualCollections"></param>
        /// <param name="hbmCumulativeIndividualDayCollection"></param>
        /// <param name="hbmCumulativeIndividualCollection"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="exposureType"></param>
        /// <param name="upperPercentage"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        public void SummarizeUncertainty(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType,
            double upperPercentage,
            double lowerBound,
            double upperBound
        ) {
            if (exposureType == ExposureType.Acute) {
                computeAcuteRiskDrivers(
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    upperPercentage,
                    lowerBound,
                    upperBound
                );
            } else {
                computeChronicRiskDrivers(
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    upperPercentage,
                    lowerBound,
                    upperBound
                );
            }
        }

        /// <summary>
        /// Compute chronic risk drivers
        /// </summary>
        /// <param name="hbmIndividualCollections"></param>
        /// <param name="hbmCumulativeIndividualCollection"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        private void computeChronicRiskDrivers(
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            double percentageForUpperTail,
            double lowerBound = double.NaN,
            double upperBound = double.NaN
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var weights = collection.HbmIndividualConcentrations
                .Select(c => c.IndividualSamplingWeight)
                .ToList();
            var percentile = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividualId).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;
            if (NumberOfIntakes > 0) {
                LowPercentileValue = upperIntakes.Select(c => c.CumulativeConcentration).Min();
                HighPercentileValue = upperIntakes.Select(c => c.CumulativeConcentration).Max();
            }
            UpperPercentage = 100 - percentageForUpperTail;
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / hbmCumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Sum(c => c.IndividualSamplingWeight) * 100;

            Records = activeSubstances.Select(substance => {
                var cumulativeConcentrations = collection.HbmIndividualConcentrations
                    .Where(c => upperIndividuals.Contains(c.SimulatedIndividualId))
                    .Select(c => c.ConcentrationsBySubstance.TryGetValue(substance, out var r)
                                ? r.Exposure * relativePotencyFactors[substance] * c.IndividualSamplingWeight
                                : 0D
                    ).ToList();
                return getRiskDriverRecord(
                    substance,
                    cumulativeConcentrations,
                    totalConcentration,
                    lowerBound,
                    upperBound
                );
            }).OrderByDescending(c => c.Contribution).ToList();
        }

        /// <summary>
        /// Compute acute risk drivers
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <param name="hbmCumulativeIndividualDayCollection"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        private void computeAcuteRiskDrivers(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            double percentageForUpperTail,
            double lowerBound = double.NaN,
            double upperBound = double.NaN
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var weights = collection.HbmIndividualDayConcentrations
                .Select(c => c.IndividualSamplingWeight)
                .ToList();

            var percentile = hbmCumulativeIndividualDayCollection
                .HbmCumulativeIndividualDayConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualDayCollection
                .HbmCumulativeIndividualDayConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividualDayId).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;

            if (NumberOfIntakes > 0) {
                LowPercentileValue = upperIntakes.Select(c => c.CumulativeConcentration).Min();
                HighPercentileValue = upperIntakes.Select(c => c.CumulativeConcentration).Max();
            }

            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / hbmCumulativeIndividualDayCollection.HbmCumulativeIndividualDayConcentrations.Sum(c => c.IndividualSamplingWeight) * 100;

            Records = activeSubstances.Select(substance => {
                var cumulativeConcentrations = collection.HbmIndividualDayConcentrations
                    .Where(c => upperIndividuals.Contains(c.SimulatedIndividualDayId))
                    .Select(c => c.ConcentrationsBySubstance.TryGetValue(substance, out var r)
                                ? r.Exposure * relativePotencyFactors[substance] * c.IndividualSamplingWeight
                                : 0D
                    ).ToList();

                return getRiskDriverRecord(
                    substance,
                    cumulativeConcentrations,
                    totalConcentration,
                    lowerBound,
                    upperBound
                );
            }).OrderByDescending(c => c.Contribution).ToList();
        }

        private HbmRiskDriverRecord getRiskDriverRecord(
            Compound substance,
            List<double> cumulativeConcentrations,
            double totalConcentration,
            double lowerBound = double.NaN,
            double upperBound = double.NaN
        ) {
            var contribution = cumulativeConcentrations.Sum(c => c) / totalConcentration * 100;
            if (double.IsNaN(lowerBound)) {
                var record = new HbmRiskDriverRecord() {
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    Contribution = contribution,
                    NumberOfConcentrations = cumulativeConcentrations.Count,
                    Contributions = new List<double>()
                };
                return record;
            } else {
                var record = Records.SingleOrDefault(c => c.SubstanceCode == substance.Code);
                record.Contributions.Add(contribution);
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                return record;
            }
        }
    }
}
