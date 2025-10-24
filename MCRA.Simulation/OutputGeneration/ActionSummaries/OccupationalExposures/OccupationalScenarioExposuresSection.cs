using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalScenarioExposuresSection : ActionSummarySectionBase {

        public List<OccupationalScenarioExposureDistributionRecord> Records { get; set; }
        public List<OccupationalScenarioExposureDistributionBoxPlotRecord> BoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<OccupationalScenarioExposure> occupationalExposures,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double lowerPercentage,
            double upperPercentage
        ) {
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            Records = occupationalExposures
                .GroupBy(r => (r.Scenario, r.Route, r.Substance))
                .Select(r => getSummaryRecord(
                    r.Select(e => e.Value).ToList(),
                    r.Key.Scenario,
                    r.Key.Route,
                    r.Key.Substance,
                    r.First().Unit,
                    percentages
                ))
                .OrderBy(c => c.ScenarioName)
                .ThenBy(c => c.ExposureRoute)
                .ThenBy(c => c.SubstanceName)
                .ToList();

            BoxPlotRecords = occupationalExposures
                .GroupBy(r => (r.Scenario, r.Route, r.Substance))
                .Select(r => getBoxPlotSummaryRecord(
                    r.Select(e => e.Value).ToList(),
                    r.Key.Scenario,
                    r.Key.Route,
                    r.Key.Substance,
                    r.First().Unit
                ))
                .OrderBy(c => c.ScenarioName)
                .ThenBy(c => c.ExposureRoute)
                .ThenBy(c => c.SubstanceName)
                .ToList();
        }

        private static OccupationalScenarioExposureDistributionRecord getSummaryRecord(
            IEnumerable<double> exposures,
            OccupationalScenario scenario,
            ExposureRoute route,
            Compound substance,
            OccupationalExposureUnit exposureUnit,
            double[] percentages
        ) {
            var weights = exposures
                .Select(c => 1D)
                .ToList();
            var percentilesAll = exposures
                .PercentilesWithSamplingWeights(weights, percentages);
            var record = new OccupationalScenarioExposureDistributionRecord {
                ScenarioCode = scenario.Code,
                ScenarioName = scenario.Name,
                ExposureRoute = route.GetShortDisplayName(),
                SubstanceName = substance.Name,
                SubstanceCode = substance.Code,
                Unit = exposureUnit.GetShortDisplayName(),
                EstimateType = exposureUnit.EstimateType != JobTaskExposureEstimateType.Undefined
                    ? exposureUnit.EstimateType.GetShortDisplayName()
                    : null,
                Mean = exposures.Select((c, ix) => c * weights[ix]).Sum() / weights.Sum(),
                LowerPercentile = percentilesAll[0],
                Median = percentilesAll[1],
                UpperPercentile = percentilesAll[2],
                MedianUncertaintyValues = []
            };
            return record;
        }

        private static OccupationalScenarioExposureDistributionBoxPlotRecord getBoxPlotSummaryRecord(
            IEnumerable<double> exposures,
            OccupationalScenario scenario,
            ExposureRoute route,
            Compound substance,
            OccupationalExposureUnit exposureUnit
        ) {
            double[] percentages = [5, 10, 25, 50, 75, 90, 95];
            var weights = exposures
                .Select(c => 1D)
                .ToList();
            var percentilesAll = exposures
                .PercentilesWithSamplingWeights(weights, percentages);
            var positives = exposures
                .Where(r => r > 0)
                .ToList();
            var record = new OccupationalScenarioExposureDistributionBoxPlotRecord() {
                ScenarioCode = scenario.Code,
                ScenarioName = scenario.Name,
                ExposureRoute = route.GetDisplayName(),
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                Percentiles = percentilesAll.ToList(),
                NumberOfPositives = positives.Count,
                Percentage = positives.Count * 100d / exposures.Count(),
                Unit = exposureUnit.GetDisplayName()
            };
            return record;
        }
    }
}
