using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation {
    public sealed class ChlorpyrifosPbkModelCalculator : DesolvePbkModelCalculator {

        public ChlorpyrifosPbkModelCalculator(
          KineticModelInstance kineticModelInstance,
          PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        protected override double getRelativeCompartmentWeight(
            TargetOutputMapping outputMapping,
            IDictionary<string, double> parameters
        ) {
            var factor = 1D;
            if (outputMapping.CompartmentId == "O_CS") {
                factor = 0.746 - parameters["VFc"] - parameters["VMc"];
            } else if (outputMapping.CompartmentId == "O_CR") {
                factor = 0.09 - parameters["VLc"] - parameters["VLuc"] - parameters["VKc"] - parameters["VHc"] - parameters["VUc"] - parameters["VBrc"];
            } else {
                foreach (var scalingFactor in outputMapping.OutputDefinition.ScalingFactors) {
                    factor *= parameters[scalingFactor];
                }
            }
            foreach (var multiplicationFactor in outputMapping.OutputDefinition.MultiplicationFactors) {
                factor *= multiplicationFactor;
            }
            return factor;
        }
    }
}
