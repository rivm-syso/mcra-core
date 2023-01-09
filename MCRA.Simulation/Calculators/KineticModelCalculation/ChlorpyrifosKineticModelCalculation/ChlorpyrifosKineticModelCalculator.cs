using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ChlorpyrifosKineticModelCalculation {
    public sealed class ChlorpyrifosKineticModelCalculator : PbpkModelCalculator {
        public ChlorpyrifosKineticModelCalculator(
          KineticModelInstance kineticModelInstance,
          Dictionary<ExposureRouteType, double> defaultAbsorptionFactors
      ) : base(kineticModelInstance, defaultAbsorptionFactors) {
        }

        protected override double getRelativeCompartmentWeight(KineticModelOutputDefinition parameter, Dictionary<string, double> parameters) {
            var factor = 1D;
            foreach (var scalingFactor in parameter.ScalingFactors) {
                factor *= parameters[scalingFactor];
            }
            return factor;
        }
    }
}
