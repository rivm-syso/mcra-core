using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctionModels.ExposureResponseSpecificationModels {

    public sealed class ExposureResponseConstantModel : ExposureResponseFactorModelBase {

        public ExposureResponseConstantModel(ExposureResponseFunction erf) : base(erf) { }

        public ExposureResponseConstantModel(
            double nominal,
            double lower,
            double upper,
            ExposureResponseSpecificationDistributionType distributionType,
            EffectMetric effectMetric
        ) : base(nominal, lower, upper, distributionType, effectMetric) {
        }

        public override double Compute(double exposure, double counterFactualValue) {
            return _draw;
        }
    }
}
