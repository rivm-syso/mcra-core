
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.ConcentrationModels {
    public class ConcentrationModelsOutputData : IModuleOutputData {
        public IDictionary<(Food Food, Compound Substance), ConcentrationModel> ConcentrationModels { get; set; }
        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels { get; set; }
        public IDictionary<(Food Food, Compound Substance), FoodSubstanceResidueCollection> CompoundResidueCollections { get; set; }
        public Dictionary<Food, FoodSubstanceResidueCollection> CumulativeCompoundResidueCollections { get; set; }
        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections { get; set; }
        public IModuleOutputData Copy() {
            return new ConcentrationModelsOutputData() {
                ConcentrationModels = ConcentrationModels,
                CumulativeConcentrationModels = CumulativeConcentrationModels,
                CompoundResidueCollections = CompoundResidueCollections,
                CumulativeCompoundResidueCollections = CumulativeCompoundResidueCollections,
                MonteCarloSubstanceSampleCollections = MonteCarloSubstanceSampleCollections
            };
        }
    }
}

