using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosKineticModelCalculation {
    public sealed class ChlorpyrifosPbkModelCalculator : DesolvePbkModelCalculator {

        public ChlorpyrifosPbkModelCalculator(
          KineticModelInstance kineticModelInstance
        ) : base(kineticModelInstance) {
        }

        protected override double getRelativeCompartmentWeight(KineticModelOutputDefinition parameter, IDictionary<string, double> parameters) {
            var factor = 1D;
            if (parameter.Id == "O_CS") {
                factor = 0.746 - parameters["VFc"] - parameters["VMc"];
            } else if (parameter.Id == "O_CR") {
                factor = 0.09 - parameters["VLc"] - parameters["VLuc"] - parameters["VKc"] - parameters["VHc"] - parameters["VUc"] - parameters["VBrc"];
            } else {
                foreach (var scalingFactor in parameter.ScalingFactors) {
                    factor *= parameters[scalingFactor];
                }
            }
            foreach (var multiplicationFactor in parameter.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
