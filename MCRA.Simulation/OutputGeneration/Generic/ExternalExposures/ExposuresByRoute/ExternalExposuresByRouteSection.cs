using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRoute {
    public class ExternalExposuresByRouteSection : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<ExternalExposuresByRouteRecord> TableRecords { get; set; } = [];
        public List<ExternalExposuresByRoutePercentilesRecord> BoxPlotRecords { get; set; } = [];
        public ExposureUnitTriple ExposureUnit { get; set; }
        public virtual string PictureId { get; } = "45142be4-4274-4869-8200-f8cde245c275";

        public void Summarize<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpf,
            IDictionary<Compound, double> membership,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit,
            SectionHeader header = null
         ) where T : IExternalIndividualExposure {
            rpf = rpf ?? substances.ToDictionary(r => r, r => 1D);
            membership = membership ?? substances.ToDictionary(r => r, r => 1D);
            ExposureUnit = exposureUnit;

            var cancelToken = ProgressState?.CancellationToken ?? new();
            TableRecords = [];
            BoxPlotRecords = [];
            foreach (var route in routes) {
                cancelToken.ThrowIfCancellationRequested();
                var record = getRecords(
                    route,
                    externalIndividualExposures,
                    substances,
                    rpf,
                    membership,
                    isPerPerson,
                    exposureUnit
                );

                if (record != default) {
                    TableRecords.Add(record.tableRecord);
                    BoxPlotRecords.Add(record.boxPlotRecord);
                }
            }

            TableRecords = [.. TableRecords
                .OrderBy(r => r.ExposureRoute)];
            BoxPlotRecords = [.. BoxPlotRecords
                .OrderBy(r => r.ExposureRoute)];

            // Generates and summarizes exposure percentiles for the specified individuals, routes, and substances,
            // incorporating uncertainty bounds, relative potency factors, and membership probabilities.
            var section = new ExposurePercentilesByRouteSection() { ProgressState = ProgressState };
            var subHeader = header?.AddSubSectionHeaderFor(section, "Percentiles", 0);
            section.SummarizeByRoute(
                externalIndividualExposures,
                routes,
                substances,
                rpf,
                membership,
                percentages,
                uncertaintyLowerBound,
                uncertaintyUpperBound,
                isPerPerson,
                exposureUnit
            );
            subHeader?.SaveSummarySection(section);
        }

        private static (ExternalExposuresByRouteRecord tableRecord, ExternalExposuresByRoutePercentilesRecord boxPlotRecord) getRecords<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            var allExposures = substances
                .SelectMany(substance => {
                    var rpf = relativePotencyFactors[substance];
                    var membership = membershipProbabilities[substance];
                    return externalIndividualExposures.Select(e => new {
                        Exposure = e.GetExposure(route, substance, isPerPerson) * rpf * membership,
                        Substance = substance,
                        e.SimulatedIndividual,
                    });
                })
                .GroupBy(c => c.SimulatedIndividual)
                .Select(g => new {
                    SimulatedIndividual = g.Key,
                    Exposure = g.Sum(c => c.Exposure),
                    Substances = g.Select(c => c.Substance).ToHashSet(),
                })
                .ToList();

            var positives = allExposures
                .Where(c => c.Exposure > 0)
                .ToList();
            if (positives.Count == 0) {
                return default;
            }

            var samplingWeightsAll = allExposures
                .Select(c => c.SimulatedIndividual.SamplingWeight)
                .ToList();
            var samplingWeightsPositives = positives
               .Select(r => r.SimulatedIndividual.SamplingWeight)
               .ToList();

            var percentilesAll = allExposures
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(samplingWeightsAll, BoxPlotChartCreatorBase.BoxPlotPercentages);
            var percentilesPositives = positives
                .Select(c => c.Exposure)
                .PercentilesWithSamplingWeights(samplingWeightsPositives, BoxPlotChartCreatorBase.BoxPlotPercentages);

            var sumAll = allExposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var sumPositives = positives.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var individualIds = externalIndividualExposures
                .Select(c => c.SimulatedIndividual.Id)
                .Distinct()
                .ToList();

            var outliers = allExposures
                .Where(c => c.Exposure > percentilesAll[4] + 3 * (percentilesAll[4] - percentilesAll[2])
                    || c.Exposure < percentilesAll[2] - 3 * (percentilesAll[4] - percentilesAll[2]))
                .Select(c => c.Exposure)
                .ToList();

            var tableRecord = new ExternalExposuresByRouteRecord {
                ExposureRoute = route.GetShortDisplayName(),
                MeanAll = sumAll / samplingWeightsAll.Sum(),
                MeanPositives = sumPositives / samplingWeightsPositives.Sum(),
                Percentile25 = percentilesPositives[2],
                MedianPositives = percentilesPositives[3],
                Percentile75 = percentilesPositives[4],
                Percentile25All = percentilesAll[2],
                MedianAll = percentilesAll[3],
                Percentile75All = percentilesAll[4],
                FractionPositive = Convert.ToDouble(positives.Count) / Convert.ToDouble(allExposures.Count),
                NumberOfSubstances = positives.SelectMany(c => c.Substances).ToHashSet().Count,
                IndividualsWithPositiveExposure = positives.Count,
                Unit = exposureUnit.GetShortDisplayName(),
            };

            var boxplotRecord = new ExternalExposuresByRoutePercentilesRecord() {
                ExposureRoute = route.GetShortDisplayName(),
                MinPositives = positives.Count != 0 ? positives.Min(p => p.Exposure) : double.NaN,
                MaxPositives = positives.Count != 0 ? positives.Max(p => p.Exposure) : double.NaN,
                Percentiles = [.. percentilesAll],
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / allExposures.Count,
                Unit = exposureUnit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };

            return (tableRecord, boxplotRecord);
        }
    }
}