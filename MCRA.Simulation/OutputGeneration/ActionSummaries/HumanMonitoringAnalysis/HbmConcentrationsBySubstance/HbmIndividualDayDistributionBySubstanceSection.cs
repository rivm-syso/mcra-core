using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmIndividualDayDistributionBySubstanceSection : SummarySection {

        public List<HbmIndividualDayDistributionBySubstanceRecord> Records { get; set; }
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }
        public string BiologicalMatrix { get; set; }

        public void Summarize(
            ICollection<HbmIndividualDayConcentration> IndividualDayConcentrations,
            ICollection<Compound> selectedSubstances,
            string biologicalMatrix,
            double lowerPercentage,
            double upperPercentage
        ) {
            BiologicalMatrix = biologicalMatrix;
            var result = new List<HbmIndividualDayDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var substance in selectedSubstances) {
                var hbmIndividualDayConcentrations = IndividualDayConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.AverageEndpointSubstanceExposure(substance),
                        sourceSamplingMethods: c.ConcentrationsBySubstance
                            .TryGetValue(substance, out var record) ? record.SourceSamplingMethods : null
                    ))
                    .ToList();

                var sourceSamplingMethods = hbmIndividualDayConcentrations
                    .SelectMany(c => c.sourceSamplingMethods)
                    .GroupBy(c => c)
                    .Select(c => $"{c.Key.Name} ({c.Count()})")
                    .ToList();

                var weights = hbmIndividualDayConcentrations
                    .Where(c => c.totalEndpointExposures > 0)
                    .Select(c => c.samplingWeight).ToList();
                var percentiles = hbmIndividualDayConcentrations
                    .Where(c => c.totalEndpointExposures > 0)
                    .Select(c => c.totalEndpointExposures)
                    .PercentilesWithSamplingWeights(weights, percentages);

                var weightsAll = hbmIndividualDayConcentrations.Select(c => c.samplingWeight).ToList();
                var percentilesAll = hbmIndividualDayConcentrations
                    .Select(c => c.totalEndpointExposures)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new HbmIndividualDayDistributionBySubstanceRecord {
                    BiologicalMatrix = biologicalMatrix,
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    MeanAll = hbmIndividualDayConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weightsAll.Sum(),
                    PercentagePositives = weights.Count / (double)IndividualDayConcentrations.Count * 100,
                    MeanPositives = hbmIndividualDayConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weights.Sum(),
                    LowerPercentilePositives = percentiles[0],
                    Median = percentiles[1],
                    UpperPercentilePositives = percentiles[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    NumberOfDays = weights.Count,
                    SourceSamplingMethods = string.Join(", ", sourceSamplingMethods)
                };
                result.Add(record);
            }
            result = result
               .Where(r => r.MeanPositives > 0)
               .ToList();
            Records = result;
            summarizeBoxPot(IndividualDayConcentrations, selectedSubstances);
        }


        private void summarizeBoxPot(
            ICollection<HbmIndividualDayConcentration> individualDayConcentrations,
            ICollection<Compound> selectedSubstances
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var multipleSamplingMethods = Records.Select(c => c.SourceSamplingMethods).Distinct().Count() > 1;
            foreach (var substance in selectedSubstances) {
                var hbmIndividualDayConcentrations = individualDayConcentrations
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.AverageEndpointSubstanceExposure(substance),
                        sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                            ? (record as IHbmSubstanceTargetExposure).SourceSamplingMethods : null
                    ))
                    .ToList();
                if (hbmIndividualDayConcentrations.Any(c => c.totalEndpointExposures > 0)) {
                    var sourceSamplingMethods = hbmIndividualDayConcentrations
                        .SelectMany(c => c.sourceSamplingMethods)
                        .GroupBy(c => c)
                        .Select(c => $"{c.Key.Name}")
                        .ToList();
                    var weights = hbmIndividualDayConcentrations
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var allExposures = hbmIndividualDayConcentrations
                        .Select(c => c.totalEndpointExposures)
                        .ToList();
                    var percentiles = allExposures
                        .PercentilesWithSamplingWeights(weights, percentages)
                        .ToList();
                    var positives = allExposures.Where(r => r > 0).ToList();
                    var record = new HbmConcentrationsPercentilesRecord() {
                        MinPositives = positives.Any() ? positives.Min() : 0,
                        MaxPositives = positives.Any() ? positives.Max() : 0,
                        SubstanceCode = substance.Code,
                        SubstanceName = substance.Name,
                        Description = multipleSamplingMethods ? $"{substance.Name} {string.Join(", ", sourceSamplingMethods)}" : substance.Name,
                        Percentiles = percentiles.ToList(),
                        NumberOfPositives = weights.Count,
                        Percentage = weights.Count * 100d / hbmIndividualDayConcentrations.Count
                    };
                    result.Add(record);
                }
            }
            HbmBoxPlotRecords = result;
        }
    }
}
