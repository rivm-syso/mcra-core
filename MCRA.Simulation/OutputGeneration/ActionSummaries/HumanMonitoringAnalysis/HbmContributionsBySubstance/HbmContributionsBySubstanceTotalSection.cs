using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmContributionsBySubstanceTotalSection
        : HbmContributionsBySubstanceSectionBase {

        public override bool SaveTemporaryData => true;

        /// <summary>
        /// Summarize nominal
        /// </summary>
        public void Summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            ExposureType exposureType
        ) {
            if (exposureType == ExposureType.Acute) {
                summarize(
                    stratifier,
                    hbmIndividualDayCollections,
                    hbmCumulativeIndividualDayCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships
                );
            } else {
                summarize(
                    stratifier,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualCollection,
                    activeSubstances,
                    relativePotencyFactors,
                    assessmentGroupMemberships
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
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );
            }
        }

        /// <summary>
        /// Compute chronic contributions by substance.
        /// </summary>
        private void summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Sum(c => c.CumulativeConcentration);

            foreach (var substance in substances) {
                var descriptor = new SubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor
                );
                var record = createContributionRecord(
                    concentrations,
                    descriptor,
                    null
                );
                Records.Add(record);

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    var stratifiedRecords = createContributionRecords(
                        concentrations,
                        descriptor,
                        true
                    );
                    Records.AddRange(stratifiedRecords);
                }
            }
        }

        /// <summary>
        /// Compute acute contributions by substance.
        /// </summary>
        private void summarize(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Sum(c => c.CumulativeConcentration);
            foreach (var substance in activeSubstances) {
                var descriptor = new SubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor
                );
                var record = createContributionRecord(
                    concentrations,
                    descriptor,
                    null
                );
                Records.Add(record);

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    var stratifiedRecords = createContributionRecords(
                        concentrations,
                        descriptor,
                        true
                    );
                    Records.AddRange(stratifiedRecords);
                }
            }
        }

        /// <summary>
        /// Compute chronic risk drivers.
        /// </summary>
        private void summarizeUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualCollection,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            var collection = hbmIndividualCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualCollection
                .HbmCumulativeIndividualConcentrations
                .Sum(c => c.CumulativeConcentration);

            foreach (var substance in activeSubstances) {
                var descriptor = new SubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor
                );
                updateRecord(
                    concentrations,
                    descriptor,
                    null,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    updateStratifiedRecords(
                        concentrations,
                        descriptor,
                        true,
                        lowerBound,
                        upperBound,
                        uncertaintyCycles
                    );
                }
            }
        }

        /// <summary>
        /// Compute acute risk drivers.
        /// </summary>
        private void summarizeUncertainty(
            PopulationStratifier stratifier,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayCollection,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            double lowerBound,
            double upperBound,
            int uncertaintyCycles
        ) {
            var collection = hbmIndividualDayCollections.FirstOrDefault();
            var totalConcentration = hbmCumulativeIndividualDayCollection
                .IndividualDayConcentrations
                .Sum(c => c.CumulativeConcentration);
            foreach (var substance in substances) {
                var descriptor = new SubstanceContributorKey() { Substance = substance };
                var concentrations = getConcentrations(
                    stratifier,
                    relativePotencyFactors,
                    assessmentGroupMemberships,
                    collection,
                    descriptor
                );
                updateRecord(
                    concentrations,
                    descriptor,
                    null,
                    lowerBound,
                    upperBound,
                    uncertaintyCycles
                );

                if (stratifier != null && stratifier.GetStratificationVariables().Count > 0) {
                    var stratifiedRecords = createContributionRecords(
                        concentrations,
                        descriptor,
                        true
                    );
                }
            }
        }
    }
}
