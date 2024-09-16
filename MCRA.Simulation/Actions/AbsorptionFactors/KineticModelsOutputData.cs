using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Actions.KineticModels {
    public class KineticModelsOutputData : IModuleOutputData {
        public ICollection<SimpleAbsorptionFactor> SimpleAbsorptionFactors { get; set; }
        public ICollection<KineticConversionFactorModel> AbsorptionFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new KineticModelsOutputData() {
                SimpleAbsorptionFactors = SimpleAbsorptionFactors,
                AbsorptionFactorModels = AbsorptionFactorModels
            };
        }
    }
}

