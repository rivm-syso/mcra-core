using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class DistributionRouteCompoundSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;
        protected double[] Percentages { get; set; }

        public List<DistributionRouteCompoundRecord> Summarize(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute RouteType, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var result = new List<DistributionRouteCompoundRecord>();
            var routes = kineticConversionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    //Note that exposures are rescaled after calculating all contributions based on absorption factors
                    var exposures = aggregateExposures
                        .OrderBy(r => r.SimulatedIndividualId)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            Exposure: c
                                .GetTotalRouteExposureForSubstance(
                                    route,
                                    substance,
                                    externalExposureUnit.IsPerUnit()
                                ) * kineticConversionFactors[(route, substance)]
                        ))
                        .ToList();

                    var weightsAll = exposures
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentilesAll = exposures
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weightsAll, Percentages);

                    var weightsPositives = exposures
                        .Where(c => c.Exposure > 0)
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var percentilesPositives = exposures
                        .Where(c => c.Exposure > 0)
                        .Select(c => c.Exposure)
                        .PercentilesWithSamplingWeights(weightsPositives, Percentages);

                    var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);

                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = total * (relativePotencyFactors?[substance] ?? double.NaN),
                        PercentagePositives = weightsPositives.Count / (double)aggregateExposures.Count * 100,
                        MeanAll = total / weightsAll.Sum(),
                        MeanPositives = total / weightsPositives.Sum(),
                        Percentile25Positives = percentilesPositives[0],
                        MedianPositives = percentilesPositives[1],
                        Percentile75Positives = percentilesPositives[2],
                        Percentile25All = percentilesAll[0],
                        MedianAll = percentilesAll[1],
                        Percentile75All = percentilesAll[2],
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        AssessmentGroupMembership = membershipProbabilities?[substance] ?? double.NaN,
                        AbsorptionFactor = kineticConversionFactors.TryGetValue((route, substance), out var factor) ? factor : double.NaN,
                        NumberOfIndividuals = weightsPositives.Count,
                        Contributions = [],
                    };
                    result.Add(record);
                }
            }
            var rescale = result.Sum(c => c.Contribution);
            result.ForEach(c => c.Contribution = c.Contribution / rescale);
            result.TrimExcess();
            return result.OrderByDescending(r => r.Contribution).ToList();
        }

        public List<DistributionRouteCompoundRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<(ExposureRoute RouteType, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var records = new List<DistributionRouteCompoundRecord>();

            var routes = kineticConversionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    //Note that exposures are rescaled after calculating all contributions based on absorption factors
                    var exposures = aggregateExposures
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .Select(c => (
                            SamplingWeight: c.IndividualSamplingWeight,
                            Exposure: c.GetTotalRouteExposureForSubstance(
                                route,
                                substance,
                                externalExposureUnit.IsPerUnit()
                            )
                        ))
                        .ToList();
                    // Multiply substance route exposures with kinetic conversion factor
                    exposures.ForEach(r => {
                        r.Exposure = r.Exposure > 0 ? r.Exposure * kineticConversionFactors[(route, substance)] : 0;
                    });

                    var record = new DistributionRouteCompoundRecord {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = exposures.Sum(c => c.Exposure * c.SamplingWeight)
                            * (relativePotencyFactors?[substance] ?? double.NaN),
                    };
                    records.Add(record);
                }
            }

            var rescale = records.Sum(c => c.Contribution);
            records.ForEach(c => c.Contribution = c.Contribution / rescale);
            records.TrimExcess();
            return records.OrderByDescending(r => r.Contribution).ToList();
        }
    }
}
