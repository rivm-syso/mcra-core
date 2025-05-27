﻿using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AttributableBodSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<AttributableBodSummaryRecord> Records { get; set; }

        public void Summarize(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases
        ) {
            Records = environmentalBurdenOfDiseases
                .SelectMany(r => getAttributableBodSummaryRecords(r))
                .ToList();
        }

        public void SummarizeUncertainty(
           List<EnvironmentalBurdenOfDiseaseResultRecord> results,
           double lowerBound,
           double upperBound
        ) {
            var resultsSummaryRecords = results
                .SelectMany(r => getAttributableBodSummaryRecords(r))
                .ToList();
            var lookup = resultsSummaryRecords.ToDictionary(r => (r.PopulationCode, r.BodIndicator, r.ErfCode, r.ExposureBinId));

            foreach (var record in Records) {
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                var resultSummaryRecord = lookup[(record.PopulationCode, record.BodIndicator, record.ErfCode, record.ExposureBinId)];
                record.UncertaintyLowerBound = lowerBound;
                record.UncertaintyUpperBound = upperBound;
                record.ResponseValues.Add(resultSummaryRecord.ResponseValue);
                record.AttributableFractions.Add(resultSummaryRecord.AttributableFraction);
                record.AttributableBods.Add(resultSummaryRecord.AttributableBod);
                record.StandardisedExposedAttributableBods.Add(resultSummaryRecord.AttributableBod / resultSummaryRecord.BinPercentage);
                record.BinPercentages.Add(resultSummaryRecord.BinPercentage);
                record.TotalBods.Add(resultSummaryRecord.TotalBod);
                record.CumulativeAttributableBods.Add(resultSummaryRecord.CumulativeAttributableBod);
                record.CumulativeStandardisedExposedAttributableBods.Add(resultSummaryRecord.CumulativeStandardisedExposedAttributableBod);
                record.Exposures.Add(resultSummaryRecord.Exposure);
            }
        }

        private static List<AttributableBodSummaryRecord> getAttributableBodSummaryRecords(
            EnvironmentalBurdenOfDiseaseResultRecord ebdResultRecord
        ) {
            var environmentalBurdenOfDiseaseResultBinRecords = ebdResultRecord.EnvironmentalBurdenOfDiseaseResultBinRecords;
            var population = ebdResultRecord.BurdenOfDisease.Population;
            var records = environmentalBurdenOfDiseaseResultBinRecords
                .Select(r => new AttributableBodSummaryRecord {
                    ExposureBinId = r.ExposureBinId,
                    PopulationCode = population.Code,
                    PopulationName = population.Name,
                    PopulationSize = population.Size > 0 ? population.Size : double.NaN,
                    BodIndicator = ebdResultRecord.BurdenOfDisease.BodIndicator.GetShortDisplayName(),
                    ErfCode = ebdResultRecord.ExposureResponseFunction.Code,
                    BinPercentage = r.ExposurePercentileBin.Percentage,
                    BinPercentages = [],
                    ExposurePercentileBin = r.ExposurePercentileBin.ToString(),
                    ExposureBin = r.ExposureBin.ToString(),
                    Exposure = r.Exposure,
                    Exposures = [],
                    TargetUnit = ebdResultRecord.TargetUnit.GetShortDisplayName(),
                    ResponseValue = r.ResponseValue,
                    ResponseValues = [],
                    AttributableFraction = r.AttributableFraction,
                    AttributableFractions = [],
                    TotalBod = r.TotalBod,
                    AttributableBod = r.AttributableBod,
                    AttributableBods = [],
                    StandardisedExposedAttributableBods = [],
                    TotalBods = [],
                    CumulativeAttributableBod = r.CumulativeAttributableBod,
                    CumulativeAttributableBods = [],
                    CumulativeStandardisedExposedAttributableBod = r.CumulativeStandardisedExposedAttributableBod,
                    CumulativeStandardisedExposedAttributableBods = [],
                })
                .ToList();
            return records;
        }
    }
}
