using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake exposure response functions.
    /// </summary>
    public static class FakeExposureResponseFunctionsGenerator {

        public static List<IExposureResponseFunctionModel> FakeExposureResponseFunctionModel(
            List<Compound> substances,
            List<Effect> effects,
            ExposureTarget target,
            ExposureUnitTriple unit,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var result = new List<IExposureResponseFunctionModel>();
            var erfTypes = Enum.GetValues(typeof(ExposureResponseType))
                .Cast<ExposureResponseType>()
                .ToList();
            var effectMetricTypes = Enum.GetValues(typeof(EffectMetric))
                .Cast<EffectMetric>()
                .Except([EffectMetric.Undefined])
                .ToList();
            foreach (var effect in effects) {
                foreach (var substance in substances) {
                    var erfType = erfTypes.DrawRandom(random);
                    var effectMetric = effectMetricTypes.DrawRandom(random);
                    var erf = FakeExposureResponseFunction(
                        $"ERF_{effect.Code}_{substance.Code}_{erfType.GetShortDisplayName()}",
                        substance,
                        effect,
                        target,
                        unit,
                        erfType,
                        effectMetric,
                        2,
                        true,
                        random
                    );
                    var record = new ExposureResponseFunctionModel(erf);
                    result.Add(record);
                }
            }
            return result;
        }

        public static ExposureResponseFunction FakeExposureResponseFunction(
            string code,
            Compound substance,
            Effect effect,
            ExposureTarget target,
            ExposureUnitTriple unit,
            ExposureResponseType erfType,
            EffectMetric metric,
            double counterfactualValue,
            bool hasUncertainty,
            IRandom random
        ) {
            var (erfString, erfStringLower, erfStringUpper) = fakeErfString(random, erfType, hasUncertainty);
            var result = new ExposureResponseFunction() {
                Code = code,
                Substance = substance,
                ExposureTarget = target,
                EffectMetric = metric,
                ExposureUnit = unit,
                Effect = effect,
                ExposureResponseSpecification = new NCalc.Expression(erfString),
                ExposureResponseSpecificationLower = new NCalc.Expression(erfStringLower),
                ExposureResponseSpecificationUpper = new NCalc.Expression(erfStringUpper),
                ExposureResponseType = erfType,
                CounterfactualValue = counterfactualValue
            };
            if (metric == EffectMetric.PositiveShift
                || metric == EffectMetric.NegativeShift
            ) {
                result.PopulationCharacteristic = Enum.GetValues(typeof(PopulationCharacteristicType))
                    .Cast<PopulationCharacteristicType>()
                    .Except([PopulationCharacteristicType.Undefined])
                    .DrawRandom(random);
                if (metric == EffectMetric.PositiveShift) {
                    switch (result.PopulationCharacteristic) {
                        case PopulationCharacteristicType.IQ:
                            result.EffectThresholdUpper = 110;
                            break;
                        case PopulationCharacteristicType.BirthWeight:
                            result.EffectThresholdUpper = 6000;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                } else if (metric == EffectMetric.NegativeShift) {
                    switch (result.PopulationCharacteristic) {
                        case PopulationCharacteristicType.IQ:
                            result.EffectThresholdLower = 90;
                            break;
                        case PopulationCharacteristicType.BirthWeight:
                            result.EffectThresholdLower = 2500;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return result;
        }

        private static (string, string, string) fakeErfString(
            IRandom random,
            ExposureResponseType erfType,
            bool hasUncertainty
        ) {
            string erfString;
            string erfStringLower = null;
            string erfStringUpper = null;
            switch (erfType) {
                case ExposureResponseType.Function:
                case ExposureResponseType.PerDoubling:
                case ExposureResponseType.PerUnit:
                case ExposureResponseType.Constant:
                default:
                    var erfValue = random.NextDouble(1, 2);
                    erfString = erfValue.ToString(CultureInfo.InvariantCulture);
                    if (hasUncertainty) {
                        var deviation = random.NextDouble(0.01, .1);
                        erfStringLower = ((1D - deviation) * erfValue).ToString(CultureInfo.InvariantCulture);
                        erfStringUpper = ((1D + deviation) * erfValue).ToString(CultureInfo.InvariantCulture);
                    }
                    break;
            }
            return (erfString, erfStringLower, erfStringUpper);
        }
    }
}
