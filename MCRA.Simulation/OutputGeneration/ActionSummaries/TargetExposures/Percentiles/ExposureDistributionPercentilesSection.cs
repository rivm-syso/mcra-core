using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureDistributionPercentilesSection : PercentileBootstrapSectionBase<IntakePercentileExposureBootstrapRecord> {

        public override bool SaveTemporaryData => true;
        public double UncertaintyLowerLimit { get; set; } = 2.5;
        public double UncertaintyUpperLimit { get; set; } = 97.5;
        public List<TargetExposurePercentileRecord> Records { get; set; } = [];

        public bool Stratify { get; set; }

        /// <summary>
        /// Summarizes the exposures for OIM,BBN,LNN0.
        /// Percentiles (output) from specified percentages (input).
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="percentages"></param>
        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier,
            Compound referenceSubstance,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double[] percentages
        ) {
            if (substances.Count == 1) {
                relativePotencyFactors = relativePotencyFactors
                    ?? substances.ToDictionary(r => r, r => 1D);
                membershipProbabilities = membershipProbabilities
                    ?? substances.ToDictionary(r => r, r => 1D);
            }

            Records = computePercentileRecords(
                aggregateIndividualExposures,
                relativePotencyFactors,
                membershipProbabilities,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                percentages,
                targetUnit
             );

            if (outputStratifier != null) {
                Stratify = true;
                var stratifiedRecords = computePercentileRecords(
                    aggregateIndividualExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentages,
                    targetUnit,
                    outputStratifier
                );
                Records.AddRange(stratifiedRecords);
            }
        }

        private List<TargetExposurePercentileRecord> computePercentileRecords(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            double[] percentages,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier = null
        ) {
            UncertaintyLowerLimit = uncertaintyLowerBound;
            UncertaintyUpperLimit = uncertaintyUpperBound;

            var exposureGroups = aggregateIndividualExposures
                .Select(c => (
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        relativePotencyFactors,
                        membershipProbabilities
                    ),
                    Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual)
                ))
                .GroupBy(c => c.Stratification)
                .ToList();

            var result = new List<TargetExposurePercentileRecord>();
            foreach (var group in exposureGroups) {
                if (group.Any(c => c.Exposure > 0)) {
                    var weights = group
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentiles = group
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var zip = percentages
                        .Zip(percentiles, (x, v) => new { X = x, V = v })
                        .ToList();
                    var records = zip
                        .Select(p => new TargetExposurePercentileRecord {
                            Stratification = group.Key?.Name,
                            UncertaintyLowerLimit = uncertaintyLowerBound,
                            UncertaintyUpperLimit = uncertaintyUpperBound,
                            XValue = p.X / 100,
                            Value = p.V,
                            Values = [],
                        })
                        .ToList();
                    result.AddRange(records);
                }
            }
            return result;
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            List<double> percentages,
            TargetUnit targetUnit,
            PopulationStratifier outputStratifier
        ) {
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);

            updatePercentileRecords(
                aggregateIndividualExposures,
                rpfs,
                memberships,
                targetUnit,
                percentages
            );

            if (outputStratifier != null) {
                updatePercentileRecords(
                    aggregateIndividualExposures,
                    rpfs,
                    memberships,
                    targetUnit,
                    percentages,
                    outputStratifier
                );
            }
        }

        private void updatePercentileRecords(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            TargetUnit targetUnit,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            var exposuresGroups = aggregateIndividualExposures
                .Select(c => (
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    Exposure: c.GetTotalExposureAtTarget(
                        targetUnit.Target,
                        rpfs,
                        memberships
                    ),
                    Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual)
                ))
                .GroupBy(c => c.Stratification)
                .ToList();

            foreach (var group in exposuresGroups) {
                if (group.Any(c => c.Exposure > 0)) {
                    var weights = group
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentiles = group
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var records = Records
                        .Where(r => r.Stratification == group.Key?.Name);
                    var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                        .ToList();
                    foreach (var item in zip) {
                        item.Record.Values.Add(item.Value);
                    }
                }
            }
        }
    }
}
