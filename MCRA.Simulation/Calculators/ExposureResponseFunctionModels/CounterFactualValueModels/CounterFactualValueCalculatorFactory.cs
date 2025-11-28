using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.CounterFactualValueModels {
    public class CounterFactualValueCalculatorFactory {

        public static ICounterFactualValueModel Create(
            ExposureResponseFunction erf
        ) {
            ICounterFactualValueModel model;
            switch (erf.CFVUncertaintyDistribution) {
                case CounterFactualValueDistributionType.Constant:
                    model = new CounterFactualValueConstantModel(erf);
                    break;
                case CounterFactualValueDistributionType.LogNormal:
                    model = new CounterFactualValueLogNormalModel(erf);
                    break;
                case CounterFactualValueDistributionType.Uniform:
                    model = new CounterFactualValueUniformModel(erf);
                    break;
                case CounterFactualValueDistributionType.Triangular:
                    model = new CounterFactualValueTriangularModel(erf);
                    break;
                default:
                    var msg = $"No counter factual value for distribution type ${erf.CFVUncertaintyDistribution}.";
                    throw new NotImplementedException(msg);
            }
            return model;
        }
    }
}
