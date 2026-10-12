using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications;
using MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation;

namespace MCRA.Simulation.Calculators.PbkModelCalculation {
    public class PbkModelCalculatorFactory {

        public static IPbkModelCalculator Create(
            KineticModelInstance modelInstance,
            PbkSimulationSettings simulationSettings,
            Dictionary<ExposureRoute, double> fallbackAbsorptionFactors
        ) {
            IPbkModelCalculator result = modelInstance.ModelType switch {
                PbkModelType.DeSolve => new CosmosKineticModelCalculator(modelInstance, simulationSettings, fallbackAbsorptionFactors),
                PbkModelType.EuroMix_Bisphenols_PBPK_model_V1 => new KarrerKineticModelCalculator(modelInstance, simulationSettings, fallbackAbsorptionFactors),
                PbkModelType.EuroMix_Bisphenols_PBPK_model_V2 => new KarrerReImplementedKineticModelCalculator(modelInstance, simulationSettings, fallbackAbsorptionFactors),
                PbkModelType.PBK_Chlorpyrifos_V1 => new ChlorpyrifosPbkModelCalculator(modelInstance, simulationSettings, fallbackAbsorptionFactors),
                PbkModelType.SBML => new SbmlPbkModelCalculator(modelInstance, simulationSettings, fallbackAbsorptionFactors),
                _ => throw new Exception($"No calculator for kinetic model code {modelInstance.IdModelDefinition}"),
            };
            return result;
        }
    }
}
