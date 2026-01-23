using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public List<EnvironmentalBurdenOfDiseaseSummaryRecord> Records { get; set; }
        public bool IsCumulative { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            bool cumulative
        ) {
            IsCumulative = cumulative;
            Records = [.. environmentalBurdenOfDiseases.Select(getEbdSummaryRecord)];
        }

        public void SummarizeUncertainty(
            List<EnvironmentalBurdenOfDiseaseResultRecord> results,
            double lowerBound,
            double upperBound
        ) {
            var resultsSummaryRecords = results
                .Select(getEbdSummaryRecord)
                .Where(c => c != null)
                .ToList();
            var lookup = resultsSummaryRecords
                .ToDictionary(r => (r.PopulationCode, r.BodIndicator, r.SourceIndicators, r.ErfCode));
            foreach (var record in Records) {
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                var resultSummaryRecord = lookup[(record.PopulationCode, record.BodIndicator, record.SourceIndicators, record.ErfCode)];
                record.TotalAttributableBods.Add(
                    resultSummaryRecord.TotalAttributableBod
                );
                record.TotalPopulationAttributableFractions.Add(
                    resultSummaryRecord.TotalPopulationAttributableFraction
                );
            }
        }

        private static EnvironmentalBurdenOfDiseaseSummaryRecord getEbdSummaryRecord(
            EnvironmentalBurdenOfDiseaseResultRecord ebdResultRecord
        ) {
            var totalAttributableBod = ebdResultRecord.EnvironmentalBurdenOfDiseaseResultBinRecords
                .Sum(bin => bin.AttributableBod);
            var totalPopulationAttributableFraction = ebdResultRecord.EnvironmentalBurdenOfDiseaseResultBinRecords
                .Sum(bin => bin.AttributableFraction * bin.ExposurePercentileBin.Percentage) / 100;
            var population = ebdResultRecord.Population;
            return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                PopulationSize = population?.Size > 0 ? population.Size : double.NaN,
                BodIndicator = ebdResultRecord.BodIndicator.GetShortDisplayName(),
                SourceIndicators = (ebdResultRecord.SourceIndicatorList.Count > 0)
                    ? string.Join(" -> ", ebdResultRecord.SourceIndicatorList)
                    : string.Empty,
                PopulationCode = ebdResultRecord.Population?.Code,
                PopulationName = ebdResultRecord.Population?.Name,
                SubstanceCode = ebdResultRecord.ExposureResponseModel.Substance.Code,
                SubstanceName = ebdResultRecord.ExposureResponseModel.Substance.Name,
                EffectCode = ebdResultRecord.Effect.Code,
                EffectName = ebdResultRecord.Effect.Name,
                ErfCode = ebdResultRecord.ExposureResponseModel.Code,
                ErfName = ebdResultRecord.ExposureResponseModel.Name,
                TotalAttributableBod = totalAttributableBod,
                TotalAttributableBods = [],
                StandardisedPopulationSize = ebdResultRecord.StandardisedPopulationSize,
                TotalPopulationAttributableFraction = totalPopulationAttributableFraction,
                TotalPopulationAttributableFractions = []
            };
        }
    }
}


