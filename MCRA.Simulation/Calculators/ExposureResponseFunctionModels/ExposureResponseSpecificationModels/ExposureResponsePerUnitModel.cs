using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public sealed class ExposureResponsePerUnitModel : ExposureResponseFactorModelBase {

        public ExposureResponsePerUnitModel(ExposureResponseFunction erf) : base(erf) { }

        public ExposureResponsePerUnitModel(
            double nominal,
            double lower,
            double upper,
            ExposureResponseSpecificationDistributionType distributionType
        ) : base(nominal, lower, upper, distributionType) {
        }

        public override double Compute(double exposure, double counterFactualValue) {
            var a = _draw;
            var b = 1 - a * counterFactualValue;
            return a * exposure + b;
        }
    }
}
