using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmIndividualDayDistributionBySubstanceSection : SummarySection {

        public List<HbmIndividualDayDistributionBySubstanceRecord> Records { get; set; }
        public Dictionary<(string BiologicalMatrix, string ExpressionType), List<HbmConcentrationsPercentilesRecord>> HbmBoxPlotRecords { get; set; } = new Dictionary<(string, string), List<HbmConcentrationsPercentilesRecord>>();
        public BiologicalMatrix BiologicalMatrix { get; set; }

        public string CreateUnitKey((string BiologicalMatrix, string ExpressionType) key) {
            return TargetUnit.CreateUnitKey(key);
        }

        public void Summarize(
            ICollection<HbmIndividualDayConcentration> IndividualDayConcentrations,
            ICollection<Compound> selectedSubstances,
            BiologicalMatrix biologicalMatrix,
            Dictionary<TargetUnit, HashSet<Compound>> hbmSubstanceTargetUnits,
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
                    BiologicalMatrix = biologicalMatrix.GetDisplayName(),
                    SubstanceName = substance.Name,
                    SubstanceCode = substance.Code,
                    Unit = hbmSubstanceTargetUnits.GetTargetUnitString(biologicalMatrix, s => s.Code == substance.Code),
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
            summarizeBoxPlotsPerTargetUnit(IndividualDayConcentrations, biologicalMatrix, hbmSubstanceTargetUnits);
        }

        private void summarizeBoxPlotsPerTargetUnit(ICollection<HbmIndividualDayConcentration> individualDayConcentrations, BiologicalMatrix biologicalMatrix, Dictionary<TargetUnit, HashSet<Compound>> hbmSubstanceTargetUnits) {
            // Create different target unit bins for the box plots
            // NOTE: this is a bit awkward logic: the subselection of substances for which the results are presented is
            //       logic for calculating the records, see MeanPositives above. The Summarize function argument of selectedSubstances still
            //       hold all substances. 
            Dictionary<TargetUnit, List<Compound>> binsPerTargetUnit = new Dictionary<TargetUnit, List<Compound>>(new TargetUnitComparer());
            foreach (var record in Records) {
                var substanceCode = record.SubstanceCode;

                if (hbmSubstanceTargetUnits.TryGetTargetUnit(biologicalMatrix, out TargetUnit targetUnit, out Compound substance, (c => c.Code == substanceCode))) {
                    if (!binsPerTargetUnit.ContainsKey(targetUnit)) {
                        binsPerTargetUnit[targetUnit] = new List<Compound>();
                    }
                    binsPerTargetUnit[targetUnit].Add(substance);
                }
            }

            foreach (var kv in binsPerTargetUnit) {
                var targetUnit = kv.Key;
                var substances = kv.Value;

                var concentrationsPercentilesRecords = summarizeBoxPlot(individualDayConcentrations, substances);
                if (concentrationsPercentilesRecords.Count > 0) {
                    HbmBoxPlotRecords.Add((BiologicalMatrix: targetUnit.BiologicalMatrix.GetDisplayName(), ExpressionType: targetUnit.ExpressionType.ToString()), concentrationsPercentilesRecords);
                }
            };
        }


        private List<HbmConcentrationsPercentilesRecord> summarizeBoxPlot(
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
            return result;
        }
    }
}
