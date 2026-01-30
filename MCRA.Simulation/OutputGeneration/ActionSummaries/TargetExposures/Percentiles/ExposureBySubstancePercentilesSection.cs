using MCRA.Data.Compiled.Objects;
using MCRA.General;
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
            List<double> percentages
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
               ? aggregateIndividualExposures
               : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        )
                    ))
                    .ToList();

                if (exposures.Any(c => c.Exposure > 0)) {
                    var weights = exposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentiles = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);

                    var zip = percentages.Zip(percentiles, (x, v) => new { X = x, V = v })
                        .ToList();

                    var records = zip.Select(p => new TargetExposurePercentileRecord {
                        UncertaintyLowerLimit = uncertaintyLowerBound,
                        UncertaintyUpperLimit = uncertaintyUpperBound,
                        XValue = p.X / 100,
                        Value = p.V,
                        Values = [],
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name
                    })
                    .ToList();
                    Records.AddRange(records);
                }
            }
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();

            foreach (var substance in substances) {
                var exposures = aggregateExposures
                    .Select(c => (
                        SimulatedIndividual: c.SimulatedIndividual,
                        Exposure: c.GetTotalExternalExposureForSubstance(
                            substance,
                            kineticConversionFactors,
                            isPerPerson
                        )
                    ))
                    .ToList();
                if (exposures.Any(c => c.Exposure > 0)) {
                    var weights = exposures
                        .Select(c => c.SimulatedIndividual.SamplingWeight)
                        .ToList();
                    var percentiles = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weights, percentages);
                    var records = Records
                        .Where(r => r.SubstanceCode == substance.Code);
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
