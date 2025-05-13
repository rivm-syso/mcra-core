using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EnvironmentalBurdenOfDiseaseSummarySection : ActionSummarySectionBase {
        public List<EnvironmentalBurdenOfDiseaseSummaryRecord> Records { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Records = environmentalBurdenOfDiseases
                .Select(r => getEbdSummaryRecord(r))
                .ToList();
        }

        public void SummarizeUncertainty(
            List<EnvironmentalBurdenOfDiseaseResultRecord> results,
            double lowerBound,
            double upperBound
        ) {
            var resultsSummaryRecords = results
                .Select(r => getEbdSummaryRecord(r))
                .ToList();
            var lookup = resultsSummaryRecords.ToDictionary(r => (r.PopulationCode, r.BodIndicator, r.ErfCode));

            foreach (var record in Records) {
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                var resultSummaryRecord = lookup[(record.PopulationCode, record.BodIndicator, record.ErfCode)];
                record.TotalAttributableBods.Add(
                    resultSummaryRecord.TotalAttributableBod
                );
            }
        }
        private static EnvironmentalBurdenOfDiseaseSummaryRecord getEbdSummaryRecord(
            EnvironmentalBurdenOfDiseaseResultRecord ebdResultRecord
        ) {
            var totalAttributableBod = ebdResultRecord.EnvironmentalBurdenOfDiseaseResultBinRecords.Sum(bin => bin.AttributableBod);
            var population = ebdResultRecord.BaselineBodIndicator.Population;
            return new EnvironmentalBurdenOfDiseaseSummaryRecord {
                PopulationSize = population.Size > 0 ? population.Size : double.NaN,
                BodIndicator = ebdResultRecord.BaselineBodIndicator.BodIndicator.GetShortDisplayName(),
                PopulationCode = ebdResultRecord.BaselineBodIndicator.Population.Code,
                PopulationName = ebdResultRecord.BaselineBodIndicator.Population.Name,
                ErfCode = ebdResultRecord.ExposureResponseFunction.Code,
                ErfName = ebdResultRecord.ExposureResponseFunction.Name,
                TotalAttributableBod = totalAttributableBod,
                TotalAttributableBods = [],
            };
        }
    }
}


