using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class ContributionByRouteSubstanceSectionBase : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<ContributionByRouteSubstanceRecord> Records { get; set; }

        public List<ContributionByRouteSubstanceRecord> getContributionRecords(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposureRoute RouteType, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            // Contributions of route and substance are calculated using the kinetic conversion
            // factors and the external exposures.
            var routes = kineticConversionFactors.Select(c => c.Key.RouteType).Distinct().ToList();
            var records = new List<ContributionByRouteSubstanceRecord>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    // Note that exposures are rescaled after calculating all contributions
                    // based on kinetic conversion factors
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

                    var weightsPositives = exposures
                        .Where(c => c.Exposure > 0)
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var total = exposures.Sum(c => c.Exposure * c.SamplingWeight);
                    var record = new ContributionByRouteSubstanceRecord {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        ExposureRoute = route.GetShortDisplayName(),
                        Contribution = total * (relativePotencyFactors?[substance] ?? double.NaN),
                        PercentagePositives = weightsPositives.Count / (double)aggregateExposures.Count * 100,
                        Mean = total / weightsAll.Sum(),
                        RelativePotencyFactor = relativePotencyFactors?[substance] ?? double.NaN,
                        NumberOfDays = weightsPositives.Count,
                        Contributions = [],
                        UncertaintyLowerBound = uncertaintyLowerBound,
                        UncertaintyUpperBound = uncertaintyUpperBound
                    };
                    records.Add(record);
                }
            }
            var rescale = records.Sum(c => c.Contribution);
            records.ForEach(c => c.Contribution = c.Contribution / rescale);
            return [.. records.OrderByDescending(r => r.Contribution)];
        }

        public List<ContributionByRouteSubstanceRecord> SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<(ExposureRoute RouteType, Compound), double> kineticConversionFactors,
            ExposureUnitTriple externalExposureUnit
        ) {
            // Contributions of route and substance are calculated using the absorption factors and the external exposures.
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            var records = new List<ContributionByRouteSubstanceRecord>();

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

                    var record = new ContributionByRouteSubstanceRecord {
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
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

        protected void updateContributions(List<ContributionByRouteSubstanceRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute
                    && c.SubstanceCode == record.SubstanceCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
