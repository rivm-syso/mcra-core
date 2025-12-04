using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.BodIndicatorModels;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.SimulatedPopulations;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {

    public class EnvironmentalBurdenOfDiseaseCalculator {

        private readonly BodApproach _bodApproach;
        private readonly EnvironmentalBodStandardisationMethod _ebdStandardisationMethod;

        public EnvironmentalBurdenOfDiseaseCalculator(
            BodApproach bodApproach,
            EnvironmentalBodStandardisationMethod ebdStandardisationMethod
        ) {
            _bodApproach = bodApproach;
            _ebdStandardisationMethod = ebdStandardisationMethod;
        }

        /// <summary>
        /// Computes the Environmental Burdens of Disease (EBDs) for all
        /// Exposure-Response Results across all Burdens of Disease.
        /// </summary>
        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute(
            ICollection<IBodIndicatorValueModel> bodIndicatorValueModels,
            Population population,
            List<ExposureResponseResult> exposureResponseResults
        ) {
            // Compute EBDs for current ERF results
            var environmentalBurdenOfDiseases = new List<EnvironmentalBurdenOfDiseaseResultRecord>();
            foreach (var exposureResponseResult in exposureResponseResults) {
                var erfEbdResults = Compute(exposureResponseResult, bodIndicatorValueModels, population);
                environmentalBurdenOfDiseases.AddRange(erfEbdResults);
            }
            return environmentalBurdenOfDiseases;
        }

        /// <summary>
        /// Computes the Environmental Burden of Disease (EBD) for a given
        /// Exposure-Response Result across all Burdens of Disease matching
        /// the effect.
        /// </summary>
        public List<EnvironmentalBurdenOfDiseaseResultRecord> Compute(
            ExposureResponseResult exposureResponseResult,
            ICollection<IBodIndicatorValueModel> bodIndicatorValueModels,
            Population population
        ) {
            var result = new List<EnvironmentalBurdenOfDiseaseResultRecord>();

            // Compute EBDs for burdens of disease for effect of ERF
            var erf = exposureResponseResult.ExposureResponseFunctionModel;

            var effectBurdensOfDisease = bodIndicatorValueModels
                .Where(r => r.BurdenOfDisease.Effect == erf.Effect)
                .ToList();
            foreach (var bodIndicatorValueModel in effectBurdensOfDisease) {
                var resultRecord = Compute(
                    exposureResponseResult,
                    bodIndicatorValueModel,
                    population
                );
                result.Add(resultRecord);
            }

            return result;
        }

        /// <summary>
        /// Computes the Environmental Burden of Disease (EBD) for a given
        /// Exposure-Response Result, Burden of Disease, and Population.
        /// </summary>
        public EnvironmentalBurdenOfDiseaseResultRecord Compute(
            ExposureResponseResult exposureResponseResult,
            IBodIndicatorValueModel bodIndicatorValueModel,
            Population population
        ) {
            var standardisedPopulationSize = _ebdStandardisationMethod switch {
                EnvironmentalBodStandardisationMethod.PER100K => 1E5,
                EnvironmentalBodStandardisationMethod.PER10K => 1E4,
                EnvironmentalBodStandardisationMethod.PER1M => 1E6,
                _ => 1E5,
            };
            var conversionFactor = getBodIndicatorConversionFactor(bodIndicatorValueModel);
            var environmentalBurdenOfDiseaseResultBinRecords = exposureResponseResult.ExposureResponseResultRecords
                .Select(r => computeBinResults(
                    r,
                    exposureResponseResult.EffectMetric,
                    population,
                    bodIndicatorValueModel,
                    conversionFactor,
                    _bodApproach,
                    standardisedPopulationSize
                ))
                .ToList();
            var sum = environmentalBurdenOfDiseaseResultBinRecords.Sum(c => c.AttributableBod);
            var cumulative = 0d;
            var sumExposed = environmentalBurdenOfDiseaseResultBinRecords
                .Where(r => r.ExposurePercentileBin.Percentage > 0)
                .Sum(r => r.AttributableBod / population.Size / r.ExposurePercentileBin.Percentage * 100);
            var cumulativeExposed = 0d;
            foreach (var record in environmentalBurdenOfDiseaseResultBinRecords) {
                cumulative += record.AttributableBod;
                cumulativeExposed += record.AttributableBod / population.Size / record.ExposurePercentileBin.Percentage * 100;
                record.CumulativeAttributableBod = cumulative / sum * 100;
                record.CumulativeStandardisedExposedAttributableBod = cumulativeExposed / sumExposed * 100;
            }
            var result = new EnvironmentalBurdenOfDiseaseResultRecord {
                BurdenOfDisease = bodIndicatorValueModel.BurdenOfDisease,
                ExposureResponseModel = exposureResponseResult.ExposureResponseFunctionModel,
                ErfDoseUnit = exposureResponseResult.ExposureResponseFunctionModel.TargetUnit.ExposureUnit,
                Substance = exposureResponseResult.ExposureResponseFunctionModel.Substance,
                TargetUnit = exposureResponseResult.TargetUnit,
                EnvironmentalBurdenOfDiseaseResultBinRecords = environmentalBurdenOfDiseaseResultBinRecords,
                StandardisedPopulationSize = standardisedPopulationSize
            };
            return result;
        }

        /// <summary>
        /// Get the burden of disease indicator conversion factor
        /// </summary>
        /// <param name="bodIndicatorValueModel"></param>
        /// <returns></returns>
        private static double getBodIndicatorConversionFactor(IBodIndicatorValueModel bodIndicatorValueModel) {
            var conversionFactor = 1d;
            if (bodIndicatorValueModel.BurdenOfDisease is DerivedBurdenOfDisease) {
                var conversions = (bodIndicatorValueModel.BurdenOfDisease as DerivedBurdenOfDisease).Conversions;
                foreach (var conversion in conversions) {
                    conversionFactor *= conversion.Value;
                }
            }
            return conversionFactor;
        }

        private EnvironmentalBurdenOfDiseaseResultBinRecord computeBinResults(
            ExposureResponseResultRecord exposureResponseResultRecord,
            EffectMetric effectMetric,
            Population population,
            IBodIndicatorValueModel bodIndicatorValueModel,
            double conversionFactor,
            BodApproach bodApproach,
            double standardisedPopulationSize
        ) {
            var totalBod = bodIndicatorValueModel.GetBodIndicatorValue()
                * conversionFactor
                * exposureResponseResultRecord.PercentileInterval.Percentage / 100;
            var responseValue = exposureResponseResultRecord.PercentileSpecificRisk;
            var result = new EnvironmentalBurdenOfDiseaseResultBinRecord {
                ExposureBinId = exposureResponseResultRecord.ExposureBinId,
                ExposureBin = exposureResponseResultRecord.ExposureInterval,
                ExposurePercentileBin = exposureResponseResultRecord.PercentileInterval,
                Exposure = exposureResponseResultRecord.ExposureLevel,
                TotalBod = totalBod,
                ResponseValue = responseValue,
                ExposureResponseResultRecord = exposureResponseResultRecord,
                StandardisedPopulationSize = standardisedPopulationSize
            };
            if (bodApproach == BodApproach.TopDown) {
                var attributableFraction = computeAttributableFraction(
                    exposureResponseResultRecord,
                    population,
                    responseValue,
                    effectMetric
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
            double responseValue,
            EffectMetric effectMetric
        ) {
            var attributableFraction = 0D;
            switch (effectMetric) {
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
                            .Single(r => r.Characteristic == exposureResponseResultRecord.ExposureResponseModel.PopulationCharacteristic);
                        var thresholdLower = (double)exposureResponseResultRecord.ExposureResponseModel.EffectThresholdLower;
                        var distribution = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic);
                        var distributionShifted = PopulationCharacteristicDistributionFactory
                            .createProbabilityDistribution(characteristic, -responseValue);
                        attributableFraction = distributionShifted.CDF(thresholdLower) - distribution.CDF(thresholdLower);
                    }
                    break;
                case EffectMetric.PositiveShift: {
                        var characteristic = population.PopulationCharacteristics
                            .Single(r => r.Characteristic == exposureResponseResultRecord.ExposureResponseModel.PopulationCharacteristic);
                        var thresholdUpper = (double)exposureResponseResultRecord.ExposureResponseModel.EffectThresholdUpper;
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
