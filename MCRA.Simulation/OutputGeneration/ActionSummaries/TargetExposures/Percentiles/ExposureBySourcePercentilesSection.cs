using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourcePercentilesSection : ExposureBySourceSectionBase {

        public override bool SaveTemporaryData => true;

        public List<TargetExposurePercentileRecord> Records { get; set; } = [];

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson,
            List<double> percentages,
            PopulationStratifier outputStratifier
        ) {
            rpfs = substances.Count > 1 ? rpfs : substances.ToDictionary(r => r, r => 1D);
            memberships = substances.Count > 1 ? memberships : substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );

            Records = computePercentileRecords(
                exposureCollection,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                percentages
            );
            if (outputStratifier != null) {
                var stratifiedRecords = computePercentileRecords(
                    exposureCollection,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    percentages,
                    outputStratifier
                );
                Records.AddRange(stratifiedRecords);
            }
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );

            updatePercentileRecords(exposureCollection, percentages);

            if (outputStratifier != null) {
                updatePercentileRecords(exposureCollection, percentages, outputStratifier);
            }
        }

        private static List<TargetExposurePercentileRecord> computePercentileRecords(
            List<(ExposureSource Source, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureCollection,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            var result = new List<TargetExposurePercentileRecord>();
            foreach (var (source, allExposures) in exposureCollection) {
                var exposureGroups = allExposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.Exposure
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
                                Source = source.GetDisplayName(),
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
            List<(ExposureSource Source, List<(SimulatedIndividual SimulatedIndividual, double Exposure)> Exposures)> exposureCollection,
            List<double> percentages,
            PopulationStratifier outputStratifier = null
        ) {
            foreach (var (source, allExposures) in exposureCollection) {
                var exposureGroups = allExposures
                    .Select(c => (
                        SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                        Stratification: outputStratifier?.GetLevel(c.SimulatedIndividual),
                        Exposure: c.Exposure
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
                        var records = Records
                            .Where(r => r.Source == source.GetDisplayName()
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
}
