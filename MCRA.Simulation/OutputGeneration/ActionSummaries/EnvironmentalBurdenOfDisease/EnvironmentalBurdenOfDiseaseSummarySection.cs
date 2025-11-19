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
            var resultsSummaryRecords =  results
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
            }
        }

        private static EnvironmentalBurdenOfDiseaseSummaryRecord getEbdSummaryRecord(
            EnvironmentalBurdenOfDiseaseResultRecord ebdResultRecord
        ) {
            var totalAttributableBod = ebdResultRecord.EnvironmentalBurdenOfDiseaseResultBinRecords
                .Sum(bin => bin.AttributableBod);
            var population = ebdResultRecord.BurdenOfDisease.Population;
            return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                PopulationSize = population?.Size > 0 ? population.Size : double.NaN,
                BodIndicator = ebdResultRecord.BurdenOfDisease.BodIndicator.GetShortDisplayName(),
                SourceIndicators = (ebdResultRecord.BurdenOfDisease is DerivedBurdenOfDisease)
                    ? string.Join(" -> ", (ebdResultRecord.BurdenOfDisease as DerivedBurdenOfDisease).Conversions.Select(r => r.FromIndicator))
                    : string.Empty,
                PopulationCode = ebdResultRecord.BurdenOfDisease.Population?.Code,
                PopulationName = ebdResultRecord.BurdenOfDisease.Population?.Name,
                SubstanceCode = ebdResultRecord.ExposureResponseFunction.Substance.Code,
                SubstanceName = ebdResultRecord.ExposureResponseFunction.Substance.Name,
                EffectCode = ebdResultRecord.BurdenOfDisease.Effect.Code,
                EffectName = ebdResultRecord.BurdenOfDisease.Effect.Name,
                ErfCode = ebdResultRecord.ExposureResponseFunction.Code,
                ErfName = ebdResultRecord.ExposureResponseFunction.Name,
                TotalAttributableBod = totalAttributableBod,
                TotalAttributableBods = [],
                StandardisedPopulationSize = ebdResultRecord.StandardisedPopulationSize
            };
        }
    }
}


