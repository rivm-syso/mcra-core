using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualDistributionBySubstanceSection : SummarySection {

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; }
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }
        public string BiologicalMatrix { get; set; }
        public void Summarize(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances,
            string biologicalMatrix,
            double lowerPercentage,
            double upperPercentage
        ) {
            BiologicalMatrix = biologicalMatrix;
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var substance in selectedSubstances) {
                var hbmIndividualConcentrations = individualConcentrations
                    .Where(r => r.ConcentrationsBySubstance.ContainsKey(substance))
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration,
                        sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                            ? record.SourceSamplingMethods : null
                    ))
                    .ToList();

                var sourceSamplingMethods = hbmIndividualConcentrations
                    .SelectMany(c => c.sourceSamplingMethods)
                    .GroupBy(c => c)
                    .Select(c => $"{c.Key.Name} ({c.Count()})")
                    .ToList();

                var weights = hbmIndividualConcentrations.Where(c => c.totalEndpointExposures > 0)
                    .Select(c => c.samplingWeight).ToList();
                var percentiles = hbmIndividualConcentrations.Where(c => c.totalEndpointExposures > 0)
                    .Select(c => c.totalEndpointExposures)
                    .PercentilesWithSamplingWeights(weights, percentages);

                var weightsAll = hbmIndividualConcentrations.Select(c => c.samplingWeight).ToList();
                var percentilesAll = hbmIndividualConcentrations
                    .Select(c => c.totalEndpointExposures)
                    .PercentilesWithSamplingWeights(weightsAll, percentages);
                var record = new HbmIndividualDistributionBySubstanceRecord {
                    BiologicalMatrix = biologicalMatrix,
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    MeanAll = hbmIndividualConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weightsAll.Sum(),
                    PercentagePositives = weights.Count / (double)individualConcentrations.Count * 100,
                    MeanPositives = hbmIndividualConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weights.Sum(),
                    LowerPercentilePositives = percentiles[0],
                    MedianPositives = percentiles[1],
                    UpperPercentilePositives = percentiles[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    IndividualsWithPositiveConcentrations = weights.Count,
                    SourceSamplingMethods = string.Join(", ", sourceSamplingMethods)
                };
                result.Add(record);
            }
            result = result
                .Where(r => r.MeanPositives > 0)
                .ToList();
            Records = result;
            summarizeBoxPot(individualConcentrations, selectedSubstances);
        }

        private void summarizeBoxPot(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var multipleSamplingMethods = Records.Select(c => c.SourceSamplingMethods).Distinct().Count() > 1;
            foreach (var substance in selectedSubstances) {
                var hbmIndividualConcentrations = individualConcentrations
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration,
                        sourceSamplingMethods: c.ConcentrationsBySubstance.TryGetValue(substance, out var record)
                            ? record.SourceSamplingMethods : null
                    ))
                    .ToList();
                if (hbmIndividualConcentrations.Any(c => c.totalEndpointExposures > 0)) {
                    var sourceSamplingMethods = hbmIndividualConcentrations
                        .SelectMany(c => c.sourceSamplingMethods)
                        .GroupBy(c => c)
                        .Select(c => $"{c.Key.Name}")
                        .ToList();
                    var weights = hbmIndividualConcentrations
                        .Select(c => c.samplingWeight)
                        .ToList();
                    var allExposures = hbmIndividualConcentrations
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
                        Percentage = weights.Count * 100d / hbmIndividualConcentrations.Count
                    };
                    result.Add(record);
                }
            }
            HbmBoxPlotRecords = result;
        }
    }
}
