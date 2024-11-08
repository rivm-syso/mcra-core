using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmTotalDistributionRiskDriversSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<HbmRiskDriverRecord> Records { get; set; }

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
        public void Summarize(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType
        ) {

            if (exposureType == ExposureType.Acute) {
                computeAcuteRiskDrivers(
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors
                );
            } else {
                computeChronicRiskDrivers(
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors
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
            double lowerBound,
            double upperBound
        ) {

            if (exposureType == ExposureType.Acute) {
                computeAcuteRiskDrivers(
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    lowerBound,
                    upperBound
                );
            } else {
                computeChronicRiskDrivers(
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
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
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        private void computeChronicRiskDrivers(
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            double lowerBound = double.NaN,
            double upperBound = double.NaN
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Sum(c => c.CumulativeConcentration);

            Records = activeSubstances.Select(substance => {
                var cumulativeConcentrations = collection.HbmIndividualConcentrations
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
        /// Compute acute riskdrivers
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <param name="hbmCumulativeIndividualDayCollection"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        private void computeAcuteRiskDrivers(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            double lowerBound = double.NaN,
            double upperBound = double.NaN
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualDayCollection
                .HbmCumulativeIndividualDayConcentrations
                .Sum(c => c.CumulativeConcentration);

            Records = activeSubstances.Select(substance => {
                var cumulativeConcentrations = collection.HbmIndividualDayConcentrations
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
           double lowerBound,
           double upperBound
        ) {
            var contribution = cumulativeConcentrations.Sum(c => c) / totalConcentration * 100;
            if (double.IsNaN(lowerBound)) {
                var record = new HbmRiskDriverRecord() {
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    Contribution = contribution,
                    NumberOfConcentrations = cumulativeConcentrations.Count(),
                    Contributions = []
                };
                return record;
            } else {
                var record = Records.SingleOrDefault(c => c.SubstanceCode == substance.Code);
                record.Contributions.Add(contribution);
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                return record;
            };
        }
    }
}
