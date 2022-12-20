using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualDistributionBySubstanceSection : SummarySection {

        public List<HbmIndividualDistributionBySubstanceRecord> Records { get; set; }
        public List<HbmConcentrationsPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void Summarize(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances,
            HumanMonitoringSamplingMethod biologicalMatrix,
            double lowerPercentage,
            double upperPercentage
        ) {
            var result = new List<HbmIndividualDistributionBySubstanceRecord>();
            var percentages = new double[] { lowerPercentage, 50, upperPercentage };
            foreach (var substance in selectedSubstances) {
                var hbmIndividualConcentrations = individualConcentrations
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration 
                    ))
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
                    ExposureRoute = biologicalMatrix.ExposureRoute,
                    BiologicalMatrix = biologicalMatrix.Compartment,
                    SamplingType = biologicalMatrix.SampleType,
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    Percentage = weights.Count / (double)individualConcentrations.Count * 100,
                    Mean = hbmIndividualConcentrations.Sum(c => c.totalEndpointExposures * c.samplingWeight) / weights.Sum(),
                    LowerPercentilePositives = percentiles[0],
                    Median = percentiles[1],
                    UpperPercentilePositives = percentiles[2],
                    LowerPercentileAll = percentilesAll[0],
                    MedianAll = percentilesAll[1],
                    UpperPercentileAll = percentilesAll[2],
                    IndividualsWithPositiveConcentrations = weights.Count,
                };
                result.Add(record);
            }
            result = result
                .Where(r => r.Mean > 0)
                .OrderBy(s => s.ExposureRoute, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records = result;
            summarizeBoxPot(individualConcentrations, selectedSubstances, biologicalMatrix);
        }

        private void summarizeBoxPot(
            ICollection<HbmIndividualConcentration> individualConcentrations,
            ICollection<Compound> selectedSubstances,
            HumanMonitoringSamplingMethod biologicalMatrix
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };

            foreach (var substance in selectedSubstances) {
                var hbmIndividualConcentrations = individualConcentrations
                    .Select(c => (
                        samplingWeight: c.Individual.SamplingWeight,
                        totalEndpointExposures: c.ConcentrationsBySubstance[substance].Concentration
                    ))
                    .ToList();
                if (hbmIndividualConcentrations.Any(c => c.totalEndpointExposures > 0)) {
                    var weights = hbmIndividualConcentrations.Select(c => c.samplingWeight).ToList();
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
                        Description = $"{substance.Name}-{biologicalMatrix.Compartment}-{biologicalMatrix.SampleType}",
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
