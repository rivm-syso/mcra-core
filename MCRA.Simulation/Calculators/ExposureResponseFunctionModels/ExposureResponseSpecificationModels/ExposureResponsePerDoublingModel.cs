using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public sealed class ExposureResponsePerDoublingModel : ExposureResponseFactorModelBase {

        public ExposureResponsePerDoublingModel(ExposureResponseFunction erf) : base(erf) { }

        public ExposureResponsePerDoublingModel(
            double nominal,
            double lower,
            double upper,
            ExposureResponseSpecificationDistributionType distributionType
        ) : base(nominal, lower, upper, distributionType) {
        }

        public override double Compute(double exposure, double counterFactualValue) {
            return Math.Pow(_draw, Math.Log2(exposure / counterFactualValue));
        }
    }
}
