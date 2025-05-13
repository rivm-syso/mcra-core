using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        public List<ExposureResponseResultRecord> ExposureResponseResults { get; set; }

        public BaselineBodIndicator BaselineBodIndicator { get; set; }

        public Population Population { get; set; }

        public BodApproach BodApproach { get; set; }

        public EnvironmentalBurdenOfDiseaseCalculator(
            List<ExposureResponseResultRecord> exposureResponseResults = null,
            BaselineBodIndicator baselineBodIndicator = null,
            Population population = null,
            BodApproach bodApproach = BodApproach.TopDown
        ) {
            ExposureResponseResults = exposureResponseResults;
            BaselineBodIndicator = baselineBodIndicator;
            Population = population;
            BodApproach = bodApproach;
        }

        public EnvironmentalBurdenOfDiseaseResultRecord Compute() {
            var populationSize = Population.Size;
            var environmentalBurdenOfDiseaseResultBinRecords = ExposureResponseResults
                .Select(compute)
                .ToList();
            var sum = environmentalBurdenOfDiseaseResultBinRecords.Sum(c => c.AttributableBod);
            var cumulative = 0d;
            var sumExposed = environmentalBurdenOfDiseaseResultBinRecords
                .Where(r => r.ExposurePercentileBin.Percentage > 0)
                .Sum(r => r.AttributableBod / populationSize / r.ExposurePercentileBin.Percentage * 100);
            var cumulativeExposed = 0d;
            foreach (var record in environmentalBurdenOfDiseaseResultBinRecords) {
                cumulative += record.AttributableBod;
                cumulativeExposed += record.AttributableBod / populationSize / record.ExposurePercentileBin.Percentage * 100;
                record.CumulativeAttributableBod = cumulative / sum * 100;
                record.CumulativeStandardisedExposedAttributableBod = cumulativeExposed / sumExposed * 100;
            }
            var result = new EnvironmentalBurdenOfDiseaseResultRecord {
                BaselineBodIndicator = BaselineBodIndicator,
                ExposureResponseFunction = ExposureResponseResults.First().ExposureResponseFunction,
                ErfDoseUnit = ExposureResponseResults.First().ExposureResponseFunction.DoseUnit,
                TargetUnit = ExposureResponseResults.First().TargetUnit,
                EnvironmentalBurdenOfDiseaseResultBinRecords = environmentalBurdenOfDiseaseResultBinRecords
            };
            return result;
        }

        private EnvironmentalBurdenOfDiseaseResultBinRecord compute(
            ExposureResponseResultRecord exposureResponseResultRecord
        ) {
            var totalBod = BaselineBodIndicator.Value
                * exposureResponseResultRecord.PercentileInterval.Percentage / 100;
            var responseValue = exposureResponseResultRecord.PercentileSpecificRisk;
            var result = new EnvironmentalBurdenOfDiseaseResultBinRecord {
                ExposureBinId = exposureResponseResultRecord.ExposureBinId,
                ExposureBin = exposureResponseResultRecord.ExposureInterval,
                ExposurePercentileBin = exposureResponseResultRecord.PercentileInterval,
                Exposure = exposureResponseResultRecord.ExposureLevel,
                TotalBod = totalBod,
                ResponseValue = responseValue,
                ExposureResponseResultRecord = exposureResponseResultRecord
            };
            if (BodApproach == BodApproach.TopDown) {
                var attributableFraction = computeAttributableFraction(exposureResponseResultRecord, responseValue);
                result.AttributableFraction = attributableFraction;
                result.AttributableBod = totalBod * attributableFraction;
            } else {
                result.AttributableBod = totalBod * responseValue * Population.Size;
            }

            return result;
        }

        private double computeAttributableFraction(
            ExposureResponseResultRecord exposureResponseResultRecord,
            double responseValue
        ) {
            var attributableFraction = 0D;
            switch (exposureResponseResultRecord.EffectMetric) {
                case EffectMetric.OddsRatio: {
                        attributableFraction = (responseValue - 1) / responseValue;
                    }
                    break;
                case EffectMetric.RelativeRisk: {
                        attributableFraction = (responseValue - 1) / responseValue;
                    }
                    break;
                case EffectMetric.NegativeShift: {
                        var characteristic = Population.PopulationCharacteristics
                            .Single(r => r.Characteristic == exposureResponseResultRecord.ExposureResponseFunction.PopulationCharacteristic);
                        var thresholdLower = (double)exposureResponseResultRecord.ExposureResponseFunction.EffectThresholdLower;
                        var distribution = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic);
                        var distributionShifted = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic, -responseValue);
                        attributableFraction = distributionShifted.CDF(thresholdLower) - distribution.CDF(thresholdLower);
                    }
                    break;
                case EffectMetric.PositiveShift: {
                        var characteristic = Population.PopulationCharacteristics
                            .Single(r => r.Characteristic == exposureResponseResultRecord.ExposureResponseFunction.PopulationCharacteristic);
                        var thresholdUpper = (double)exposureResponseResultRecord.ExposureResponseFunction.EffectThresholdUpper;
                        var distribution = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic);
                        var distributionShifted = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic, responseValue);
                        attributableFraction = distributionShifted.CDF(thresholdUpper) - distribution.CDF(thresholdUpper);
                    }
                    break;
                default: {
                        throw new NotImplementedException();
                    }
            }
            return attributableFraction;
        }
    }
}
