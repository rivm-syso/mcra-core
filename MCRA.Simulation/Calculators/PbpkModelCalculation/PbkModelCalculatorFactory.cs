using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.SbmlModelCalculation;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public class PbkModelCalculatorFactory {

        public static IPbkModelCalculator Create(
            KineticModelInstance modelInstance,
            PbkSimulationSettings simulationSettings
        ) {
            IPbkModelCalculator result = modelInstance.KineticModelType switch {
                KineticModelType.DeSolve => new CosmosKineticModelCalculator(modelInstance, simulationSettings),
                KineticModelType.EuroMix_Bisphenols_PBPK_model_V1 => new KarrerKineticModelCalculator(modelInstance, simulationSettings),
                KineticModelType.EuroMix_Bisphenols_PBPK_model_V2 => new KarrerReImplementedKineticModelCalculator(modelInstance, simulationSettings),
                KineticModelType.PBK_Chlorpyrifos_V1 => new ChlorpyrifosPbkModelCalculator(modelInstance, simulationSettings),
                KineticModelType.SBML => new SbmlPbkModelCalculator(modelInstance, simulationSettings),
                _ => throw new Exception($"No calculator for kinetic model code {modelInstance.IdModelDefinition}"),
            };
            return result;
        }
    }
}
