using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRouteSubstance {
    public class ExternalExposuresByRouteSubstanceSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ExternalExposuresByRouteSubstanceRecord> TableRecords { get; set; } = [];
        public SerializableDictionary<ExposureRoute, List<ExternalExposuresByRouteSubstancePercentilesRecord>> BoxPlotRecords { get; set; } = [];
        public ExposureUnitTriple ExposureUnit { get; set; }
        public virtual string PictureId { get; } = "6b9d4690-2754-4aab-984f-f2ace1f3836e";

        public void Summarize<T>(
            ICollection<T> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);
            ExposureUnit = exposureUnit;

            var cancelToken = ProgressState?.CancellationToken ?? new();
            var externalExposuresByRouteRecords = new List<ExternalExposuresByRouteSubstanceRecord>();
            foreach (var route in routes) {
                foreach (var substance in substances) {
                    cancelToken.ThrowIfCancellationRequested();
                    var records = getRecords(
                        route,
                        externalIndividualExposures,
                        substance,
                        rpfs,
                        memberships,
                        isPerPerson,
                        exposureUnit
                    );

                    if (records != default) {
                        TableRecords.Add(records.tableRecord);
                        BoxPlotRecords.AddOrAppend(route, records.boxplotRecord);
                    }

                }
            }
            TableRecords = [.. TableRecords
                .OrderBy(r => r.ExposureRoute)
                .ThenBy(r => r.SubstanceName)];
            BoxPlotRecords =
                new SerializableDictionary<ExposureRoute, List<ExternalExposuresByRouteSubstancePercentilesRecord>>(
                    BoxPlotRecords.Keys
                        .OrderBy(r => r.GetShortDisplayName())
                        .ToDictionary(
                            k => k,
                            k => BoxPlotRecords[k]
                                .OrderBy(r => r.SubstanceName)
                                .ToList()
                ));
        }

        private static (ExternalExposuresByRouteSubstanceRecord tableRecord, ExternalExposuresByRouteSubstancePercentilesRecord boxplotRecord) getRecords<T>(
            ExposureRoute route,
            ICollection<T> externalIndividualExposures,
            Compound substance,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson,
            ExposureUnitTriple exposureUnit
        ) where T : IExternalIndividualExposure {
            var rpf = rpfs[substance];
            var membership = memberships[substance];
            var allExposures = externalIndividualExposures
                .Select(
                    r => (
                        Exposure: r.GetExposure(route, substance, isPerPerson) * rpf * membership,
                        r.SimulatedIndividual
                ))
                .ToList();

            var positives = allExposures
                .Where(e => e.Exposure > 0)
                .ToList();
            if (positives.Count == 0) {
                return default;
            }

            var weightsAll = allExposures
               .Select(r => r.SimulatedIndividual.SamplingWeight)
               .ToList();
            var weightsPositives = positives
                .Where(r => r.Exposure > 0)
                .Select(r => r.SimulatedIndividual.SamplingWeight)
                .ToList();

            var percentilesAll = allExposures
                .Select(r => r.Exposure)
                .PercentilesWithSamplingWeights(
                    weightsAll,
                    BoxPlotChartCreatorBase.BoxPlotPercentages
                );
            var percentilesPositives = positives
                .Select(r => r.Exposure)
                .PercentilesWithSamplingWeights(
                    weightsPositives,
                    BoxPlotChartCreatorBase.BoxPlotPercentages
                );

            var sumAll = allExposures.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);
            var sumPositives = positives.Sum(c => c.Exposure * c.SimulatedIndividual.SamplingWeight);

            var tableRecord = new ExternalExposuresByRouteSubstanceRecord {
                ExposureRoute = route.GetDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                IndividualsWithPositiveExposure = positives.Count,
                MeanAll = sumAll / weightsAll.Sum(),
                MeanPositives = sumPositives / weightsPositives.Sum(),
                PercentagePositives = positives.Count * 100d / allExposures.Count,
                LowerPercentilePositives = percentilesPositives[2],
                MedianPositives = percentilesPositives[3],
                UpperPercentilePositives = percentilesPositives[4],
                LowerPercentileAll = percentilesAll[2],
                MedianAll = percentilesAll[3],
                UpperPercentileAll = percentilesAll[4],
                MedianAllUncertaintyValues = [],
                Unit = exposureUnit.GetShortDisplayName()
            };

            var p95Idx = BoxPlotChartCreatorBase.BoxPlotPercentages.Length - 1;
            var outliers = allExposures
                    .Where(c => c.Exposure > percentilesAll[4] + 3 * (percentilesAll[4] - percentilesAll[2])
                        || c.Exposure < percentilesAll[2] - 3 * (percentilesAll[4] - percentilesAll[2]))
                    .Select(c => c.Exposure)
                    .ToList();
            var substanceName = percentilesAll[p95Idx] > 0 ? substance.Name : $"{substance.Name} *";
            var boxplotRecordecord = new ExternalExposuresByRouteSubstancePercentilesRecord() {
                ExposureRoute = route.GetShortDisplayName(),
                MinPositives = positives.Count != 0 ? positives.Min(p => p.Exposure) : double.NaN,
                MaxPositives = positives.Count != 0 ? positives.Max(p => p.Exposure) : double.NaN,
                SubstanceCode = substance.Code,
                SubstanceName = substanceName,
                Percentiles = [.. percentilesAll],
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / allExposures.Count,
                Unit = exposureUnit.GetShortDisplayName(),
                Outliers = outliers,
                NumberOfOutLiers = outliers.Count,
            };
            return (tableRecord, boxplotRecordecord);
        }
    }
}
