using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CounterFactualValueModels;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        private readonly BodApproach _bodApproach;
        private readonly List<double> _defaultBinBoundaries;
        private readonly ExposureGroupingMethod _exposureGroupingMethod;

        public EnvironmentalBurdenOfDiseaseCalculator(
            BodApproach bodApproach,
            ExposureGroupingMethod exposureGroupingMethod,
            List<double> defaultBinBoundaries
        ) {
            _bodApproach = bodApproach;
            _exposureGroupingMethod = exposureGroupingMethod;
            _defaultBinBoundaries = defaultBinBoundaries;
        }

        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute(
            Dictionary<ExposureTarget, (List<ITargetIndividualExposure> Exposures, TargetUnit Unit)> exposuresCollections,
            List<BurdenOfDisease> burdensOfDisease,
            Population selectedPopulation,
            ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
            ICollection<ICounterFactualValueModel> counterFactualModels
        ) {
            var environmentalBurdenOfDiseases = new List<EnvironmentalBurdenOfDiseaseResultRecord>();
            var percentileIntervals = generatePercentileIntervals(_defaultBinBoundaries);
            foreach (var exposureResponseFunctionModel in exposureResponseFunctionModels) {
                var erf = exposureResponseFunctionModel.ExposureResponseFunction;
                var cfvModel = counterFactualModels.FirstOrDefault(c => c.ExposureResponseFunction == erf);
                // Get exposures for target
                if (!exposuresCollections.TryGetValue(erf.ExposureTarget, out var targetExposures)) {
                    var msg = $"Failed to compute effects for exposure response function {erf.Code}: missing estimates for target {erf.ExposureTarget.GetDisplayName()}.";
                    throw new Exception(msg);
                }
                (var exposures, var exposureUnit) = (targetExposures.Exposures, targetExposures.Unit);

                // Compute exposure response results
                var exposureResponseCalculator = new ExposureResponseCalculator(
                    exposureResponseFunctionModel,
                    cfvModel
                );
                var exposureResponseResults = exposureResponseCalculator
                    .Compute(
                        exposures,
                        exposureUnit,
                        percentileIntervals,
                        _exposureGroupingMethod
                    );

                // Compute EBDs for burdens of disease for effect of ERF
                var effectBurdensOfDisease = burdensOfDisease
                    .Where(r => r.Effect == erf.Effect)
                    .ToList();
                foreach (var burdenOfDisease in effectBurdensOfDisease) {
                    var resultRecord = computeSingle(
                        exposureResponseResults,
                        burdenOfDisease,
                        selectedPopulation
                    );
                    environmentalBurdenOfDiseases.Add(resultRecord);
                }
            }

            return environmentalBurdenOfDiseases;
        }

        private EnvironmentalBurdenOfDiseaseResultRecord computeSingle(
            List<ExposureResponseResultRecord> exposureResponseResults,
            BurdenOfDisease burdenOfDisease,
            Population population
        ) {
            var populationSize = population?.Size ?? double.NaN;
            var environmentalBurdenOfDiseaseResultBinRecords = exposureResponseResults
                .Select(r => compute(r, population, burdenOfDisease, _bodApproach))
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
                BurdenOfDisease = burdenOfDisease,
                ExposureResponseFunction = exposureResponseResults.First().ExposureResponseFunction,
                ErfDoseUnit = exposureResponseResults.First().ExposureResponseFunction.ExposureUnit,
                TargetUnit = exposureResponseResults.First().TargetUnit,
                EnvironmentalBurdenOfDiseaseResultBinRecords = environmentalBurdenOfDiseaseResultBinRecords
            };
            return result;
        }

        private List<PercentileInterval> generatePercentileIntervals(List<double> binBoundaries) {
            if (binBoundaries.Any(r => r <= 0 || r >= 100)) {
                throw new Exception("Incorrect bin boundaries specified for EBD calculations, all bin boundaries should be greater than 0 and less than 100.");
            }

            // Make sure the specified boundaries are ordered
            binBoundaries = [.. binBoundaries
                .Where(r => r > 0 && r < 100)
                .Order()
            ];

            var lowerBinBoudaries = new List<double>(binBoundaries);
            lowerBinBoudaries.Insert(0, 0D);
            var upperBinBoudaries = new List<double>(binBoundaries) { 100D };

            var percentileIntervals = lowerBinBoudaries
                .Zip(upperBinBoudaries)
                .Select(r => new PercentileInterval(r.First, r.Second))
                .ToList();
            return percentileIntervals;
        }

        private EnvironmentalBurdenOfDiseaseResultBinRecord compute(
            ExposureResponseResultRecord exposureResponseResultRecord,
            Population population,
            BurdenOfDisease burdenOfDisease,
            BodApproach bodApproach
        ) {
            var totalBod = burdenOfDisease.Value
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
            if (bodApproach == BodApproach.TopDown) {
                var attributableFraction = computeAttributableFraction(
                    exposureResponseResultRecord,
                    population,
                    responseValue
                );
                result.AttributableFraction = attributableFraction;
                result.AttributableBod = totalBod * attributableFraction;
            } else {
                result.AttributableBod = totalBod * responseValue * population.Size;
            }

            return result;
        }

        private double computeAttributableFraction(
            ExposureResponseResultRecord exposureResponseResultRecord,
            Population population,
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
                        var characteristic = population.PopulationCharacteristics
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
                        var characteristic = population.PopulationCharacteristics
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
