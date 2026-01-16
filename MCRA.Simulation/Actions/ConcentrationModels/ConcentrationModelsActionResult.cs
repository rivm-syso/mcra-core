using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Actions.ConcentrationModels {
    public sealed class ConcentrationModelsActionResult : IActionResult {
        public IDictionary<(Food Food, Compound Substance), FoodSubstanceResidueCollection> CompoundResidueCollections { get; set; }
        public IDictionary<(Food Food, Compound Substance), ConcentrationModel> ConcentrationModels { get; set; }
        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels { get; set; }
        public ICollection<MarginalOccurrencePattern> SimulatedOccurrencePatterns { get; set; }
        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
