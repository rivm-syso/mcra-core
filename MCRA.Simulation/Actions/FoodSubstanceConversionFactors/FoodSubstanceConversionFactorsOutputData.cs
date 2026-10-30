using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FoodSubstanceConversionFactors {
    public class FoodSubstanceConversionFactorsOutputData : IModuleOutputData {
        public ICollection<DeterministicSubstanceConversionFactor> FoodSubstanceConversionFactors { get; set; }
        public IModuleOutputData Copy() {
            return new FoodSubstanceConversionFactorsOutputData() {
                FoodSubstanceConversionFactors = FoodSubstanceConversionFactors,
            };
        }
    }
}

