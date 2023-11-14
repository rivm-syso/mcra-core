using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ContributionsForIndividualsSection : SummarySection {

        public override bool SaveTemporaryData => true;
        public List<HbmSampleConcentrationPercentilesRecord> HbmBoxPlotRecords { get; set; }

        public void SummarizeBoxPlots(
            List<IndividualEffect> individualEffects,
            List<(ExposureTarget Target, Dictionary<Compound, List<IndividualEffect>> SubstanceIndividualEffects)> individualEffectsBySubstances
        ) {
            var ratioSumByIndividual = individualEffects
                .Select(c => (
                    Sum: c.ExposureHazardRatio,
                    SimulatedIndividualId: c.SimulatedIndividualId
                ))
                .ToDictionary(c => c.SimulatedIndividualId, c => c.Sum);

            var records = new List<HbmSampleConcentrationPercentilesRecord>();
            foreach (var targetCollection in individualEffectsBySubstances) {
                foreach (var targetSubstanceIndividualEffects in targetCollection.SubstanceIndividualEffects) {
                    var contributions = targetSubstanceIndividualEffects.Value
                        .Select(c => c.ExposureHazardRatio / ratioSumByIndividual[c.SimulatedIndividualId] * 100)
                        .ToList();
                    var samplingWeights = targetSubstanceIndividualEffects.Value
                        .Select(c => c.SamplingWeight)
                        .ToList();
                    var record = summarizeBoxPlot(
                        targetSubstanceIndividualEffects.Key,
                        contributions,
                        samplingWeights
                    );
                    records.Add(record);
                }
            }

            HbmBoxPlotRecords = records;
        }

        private HbmSampleConcentrationPercentilesRecord summarizeBoxPlot(
            Compound substance,
            List<double> individualContributions,
            List<double> samplingWeights
        ) {
            var result = new List<HbmConcentrationsPercentilesRecord>();
            var percentages = new double[] { 5, 10, 25, 50, 75, 90, 95 };
            var percentiles = individualContributions
                .PercentilesWithSamplingWeights(samplingWeights, percentages)
                .ToList();
            var positives = individualContributions
                .Where(r => r > 0)
                .ToList();
            var outliers = positives
                .Where(c => c > percentiles[4] + 3 * (percentiles[4] - percentiles[2])
                    || c < percentiles[2] - 3 * (percentiles[4] - percentiles[2]))
                .Select(c => c).ToList();
            var record = new HbmSampleConcentrationPercentilesRecord() {
                MinPositives = positives.Any() ? positives.Min() : 0,
                MaxPositives = positives.Any() ? positives.Max() : 0,
                SubstanceCode = substance.Code,
                SubstanceName = substance.Name,
                Description = substance.Name,
                Percentiles = percentiles.ToList(),
                NumberOfPositives = samplingWeights.Count,
                Percentage = samplingWeights.Count * 100d / individualContributions.Count,
                Outliers = outliers.ToList(),
            };
            return record;
        }
    }
}
