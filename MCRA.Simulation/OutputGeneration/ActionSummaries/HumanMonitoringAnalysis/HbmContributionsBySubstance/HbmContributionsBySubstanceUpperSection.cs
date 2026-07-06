using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmContributionsBySubstanceUpperSection
        : HbmContributionsBySubstanceSectionBase {

        public override bool SaveTemporaryData => true;
        public double UpperPercentage { get; set; }

        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }
        public double CalculatedUpperPercentage { get; set; }

        public void Summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            ExposureType exposureType,
            double percentageForUpperTail
        ) {
            if (exposureType == ExposureType.Acute) {
                summarize(
                    stratifier,
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    percentageForUpperTail
                );
            } else {
                summarize(
                    stratifier,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    percentageForUpperTail
                );
            }
        }

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        public void SummarizeUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            ExposureType exposureType,
            double upperPercentage,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            if (exposureType == ExposureType.Acute) {
                summarizeUncertainty(
                    stratifier,
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    upperPercentage,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );
            } else {
                summarizeUncertainty(
                    stratifier,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    upperPercentage,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );
            }
        }

        /// <summary>
        /// Compute chronic
        /// </summary>
        private void summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double percentageForUpperTail
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var weights = collection.HbmIndividualConcentrations
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentile = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividual.Id).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;
            if (NumberOfIntakes > 0) {
                var hbmUpperExposures = upperIntakes
                    .Select(c => c.CumulativeConcentration)
                    .ToList();
                LowPercentileValue = hbmUpperExposures.Min();
                HighPercentileValue = hbmUpperExposures.Max();
            }
            UpperPercentage = 100 - percentageForUpperTail;
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight)
                / hbmCumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            foreach (var substance in substances) {
                var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor,
                    upperIndividuals
                );
                var record = createContributionRecord(
                    concentrations,
                    descriptor,
                    null
                );
                Records.Add(record);
            }
        }

        /// <summary>
        /// Compute chronic uncertainty
        /// </summary>
        private void summarizeUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double percentageForUpperTail,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var weights = collection.HbmIndividualConcentrations
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var percentile = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividual.Id).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;
            if (NumberOfIntakes > 0) {
                var hbmUpperExposures = upperIntakes
                    .Select(c => c.CumulativeConcentration)
                    .ToList();
                LowPercentileValue = hbmUpperExposures.Min();
                HighPercentileValue = hbmUpperExposures.Max();
            }
            UpperPercentage = 100 - percentageForUpperTail;
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / hbmCumulativeIndividualCollection.HbmCumulativeIndividualConcentrations.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            foreach (var substance in activeSubstances) {
                var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor,
                    upperIndividuals
                );
                updateRecord(
                    concentrations,
                    descriptor,
                    null,
                    lowerBound,
                    upperBound, 
                    uncertaintyCycles
                );
            }
        }

        /// <summary>
        /// Compute acute
        /// </summary>
        private void summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double percentageForUpperTail
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var weights = collection.HbmIndividualDayConcentrations
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var percentile = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividualDayId).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;

            if (NumberOfIntakes > 0) {
                var hbmUpperExposures = upperIntakes
                    .Select(c => c.CumulativeConcentration)
                    .ToList();
                LowPercentileValue = hbmUpperExposures.Min();
                HighPercentileValue = hbmUpperExposures.Max();
            }

            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / hbmCumulativeIndividualDayCollection.IndividualDayConcentrations.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;
            foreach (var substance in activeSubstances) {
                var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor,
                    upperIndividuals
                );
                var record = createContributionRecord(
                    concentrations,
                    descriptor,
                    null
                );
                Records.Add(record);
            }
        }

        /// <summary>
        /// Summarize acute uncertainty
        /// </summary>
        private void summarizeUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double percentageForUpperTail,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            UpperPercentage = 100 - percentageForUpperTail;
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var weights = collection.HbmIndividualDayConcentrations
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();

            var percentile = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Select(c => c.CumulativeConcentration)
                .PercentilesWithSamplingWeights(weights, percentageForUpperTail);

            var upperIntakes = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Where(c => c.CumulativeConcentration >= percentile)
                .ToList();

            var upperIndividuals = upperIntakes.Select(c => c.SimulatedIndividualDayId).ToHashSet();
            var totalConcentration = upperIntakes.Sum(c => c.CumulativeConcentration);

            NumberOfIntakes = upperIntakes.Count;

            if (NumberOfIntakes > 0) {
                var hbmUpperExposures = upperIntakes
                    .Select(c => c.CumulativeConcentration)
                    .ToList();
                LowPercentileValue = hbmUpperExposures.Min();
                HighPercentileValue = hbmUpperExposures.Max();
            }

            CalculatedUpperPercentage = upperIntakes.Sum(c => c.SimulatedIndividual.SamplingWeight) / hbmCumulativeIndividualDayCollection.IndividualDayConcentrations.Sum(c => c.SimulatedIndividual.SamplingWeight) * 100;

            foreach (var substance in activeSubstances) {
                var descriptor = new HbmSubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor,
                    upperIndividuals
                );
                updateRecord(
                    concentrations,
                    descriptor,
                    null,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );
            }
        }
    }
}
