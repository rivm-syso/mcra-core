using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ChlorpyrifosKineticModelCalculation {
    public sealed class ChlorpyrifosKineticModelCalculator : PbpkModelCalculator {
        /// <summary>
        /// Calculate the relative compartment 
        /// </summary>
        /// <param name="kineticModelInstance"></param>
        /// <param name="defaultAbsorptionFactors"></param>
        public ChlorpyrifosKineticModelCalculator(
          KineticModelInstance kineticModelInstance,
          IDictionary<ExposurePathType, double> defaultAbsorptionFactors
      ) : base(kineticModelInstance, defaultAbsorptionFactors) {
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
