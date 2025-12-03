using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {
    public class BodIndicatorValueCalculatorFactory {

        public static IBodIndicatorValueModel Create(
            BurdenOfDisease bod
        ) {
            IBodIndicatorValueModel model;
            switch (bod.BodUncertaintyDistribution) {
                case BodIndicatorDistributionType.Constant:
                    model = new BodIndicatorValueConstantModel(bod);
                    break;
                case BodIndicatorDistributionType.LogNormal:
                    model = new BodIndicatorValueLogNormalModel(bod);
                    break;
                case BodIndicatorDistributionType.Normal:
                    model = new BodIndicatorValueNormalModel(bod);
                    break;
                case BodIndicatorDistributionType.Triangular:
                    model = new BodIndicatorValueTriangularModel(bod);
                    break;
                default:
                    var msg = $"No burden of disease indicator value for distribution type ${bod.BodUncertaintyDistribution}.";
                    throw new NotImplementedException(msg);
            }
            model.CalculateParameters();

            return model;
        }
    }
}
