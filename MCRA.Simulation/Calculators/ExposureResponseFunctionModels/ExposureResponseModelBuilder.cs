using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using NCalc;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels {
    public class ExposureResponseModelBuilder {

        public static ExposureResponseModel Create(
            ExposureResponseFunction erf
        ) {
            var result = new ExposureResponseModel() {
                CounterFactualValueModel = CounterFactualValueCalculatorFactory.Create(erf),
                ErfModel = Create(
                    erf.ExposureResponseType,
                    erf.ExposureResponseSpecification,
                    erf.ExposureResponseSpecificationLower,
                    erf.ExposureResponseSpecificationUpper,
                    erf.ERFUncertaintyDistribution,
                    erf.EffectMetric
                ),
                ExposureResponseFunction = erf,
                ErfSubGroupModels = [.. erf.ErfSubgroups
                    .Select(r => (
                        Upper: r.ExposureUpper,
                        ErfModel: Create(
                            erf.ExposureResponseType,
                            r.ExposureResponseSpecification,
                            r.ExposureResponseSpecificationLower,
                            r.ExposureResponseSpecificationUpper,
                            r.ERFUncertaintyDistribution,
                            erf.EffectMetric
                        )
                    ))]
            };
            return result;
        }

        public static IExposureResponseFunctionModel Create(
            ExposureResponseType exposureResponseType,
            Expression nominal,
            Expression lower,
            Expression upper,
            ExposureResponseSpecificationDistributionType distributionType,
            EffectMetric effectMetric
        ) {
            IExposureResponseFunctionModel exposureResponseFunctionModel = null;
            switch (exposureResponseType) {
                case ExposureResponseType.Function:
                    exposureResponseFunctionModel = new ExposureResponseFunctionModel(
                        nominal,
                        lower,
                        upper,
                        distributionType,
                        effectMetric
                    );
                    break;
                case ExposureResponseType.PerDoubling:
                    exposureResponseFunctionModel = new ExposureResponsePerDoublingModel(
                        getValue(nominal),
                        getValue(lower),
                        getValue(upper),
                        distributionType,
                        effectMetric
                    );
                    break;
                case ExposureResponseType.PerUnit:
                    exposureResponseFunctionModel = new ExposureResponsePerUnitModel(
                        getValue(nominal),
                        getValue(lower),
                        getValue(upper),
                        distributionType,
                        effectMetric
                    );
                    break;
                case ExposureResponseType.Constant:
                    exposureResponseFunctionModel = new ExposureResponseConstantModel(
                        getValue(nominal),
                        getValue(lower),
                        getValue(upper),
                        distributionType,
                        effectMetric
                    );
                    break;
                default:
                    break;
            }
            return exposureResponseFunctionModel;
        }

        private static double getValue(Expression expression) {
            if (string.IsNullOrEmpty(expression?.ExpressionString)) {
                return double.NaN;
            }
            return Convert.ToDouble(expression.Evaluate());
        }
    }
}
