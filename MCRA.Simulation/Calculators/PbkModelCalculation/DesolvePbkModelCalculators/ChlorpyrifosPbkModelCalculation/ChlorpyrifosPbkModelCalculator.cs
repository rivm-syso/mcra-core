using MCRA.Data.Compiled.Objects;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation {
    public sealed class ChlorpyrifosPbkModelCalculator : DesolvePbkModelCalculator {

        public ChlorpyrifosPbkModelCalculator(
          KineticModelInstance kineticModelInstance,
          PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        protected override double getRelativeCompartmentWeight(
            DeSolvePbkModelOutputSpecification outputSpecification,
            IDictionary<string, double> parameters
        ) {
            var factor = 1D;
            if (outputSpecification.IdCompartment == "O_CS") {
                factor = 0.746 - parameters["VFc"] - parameters["VMc"];
            } else if (outputSpecification.IdCompartment == "O_CR") {
                factor = 0.09 - parameters["VLc"] - parameters["VLuc"] - parameters["VKc"] - parameters["VHc"] - parameters["VUc"] - parameters["VBrc"];
            } else {
                foreach (var scalingFactor in outputSpecification.ScalingFactors) {
                    factor *= parameters[scalingFactor];
                }
            }
            foreach (var multiplicationFactor in outputSpecification.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
