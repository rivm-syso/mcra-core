using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySubstancePercentilesSection : SummarySection {

        public override bool SaveTemporaryData => true;

        public List<TargetExposurePercentileRecord> Records { get; set; } = [];

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            List<double> percentages,
            PopulationStratifier outputStratifier
        ) {
            var aggregateExposures = aggregateIndividualExposures
                ?? [.. aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>()];

            Records = computePercentileRecords(
                aggregateExposures,
                substances,
                kineticConversionFactors,
                isPerPerson,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                percentages
            );

            if (outputStratifier != null) {
                var stratifiedRecords = computePercentileRecords(
                    aggregateExposures,
                    substances,
                    kineticConversionFactors,
                    isPerPerson,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentages,
                    outputStratifier
                );
                Records.AddRange(stratifiedRecords);
            }
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            updatePercentileRecords(
                aggregateExposures,
                substances,
                kineticConversionFactors,
                isPerPerson,
                percentages
            );

            if (outputStratifier != null) {
                updatePercentileRecords(
                    aggregateExposures,
                    substances,
                    kineticConversionFactors,
                    isPerPerson,
                    percentages,
                    outputStratifier
                );
            }
        }

        private List<TargetExposurePercentileRecord> computePercentileRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            var result = new List<TargetExposurePercentileRecord>();
            foreach (var substance in substances) {
                var exposureGroups = aggregateExposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        )
                    ))
                    .GroupBy(c => c.Stratification)
                    .ToList();
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
                                SubstanceCode = substance.Code,
                                SubstanceName = substance.Name,
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
            }
            return result;
        }

        private void updatePercentileRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            bool isPerPerson,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            foreach (var substance in substances) {
                var exposureGroups = aggregateExposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        )
                    ))
                    .GroupBy(c => c.Stratification)
                    .ToList();
                foreach (var group in exposureGroups) {
                    var exposures = group.Select(c => (c.SamplingWeight, c.Exposure)).ToList();
                    var weights = exposures
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentiles = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var records = Records
                        .Where(r => r.SubstanceCode == substance.Code
                            && r.Stratification == group.Key?.Name);
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
