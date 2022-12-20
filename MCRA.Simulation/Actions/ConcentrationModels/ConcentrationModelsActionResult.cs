using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using System.Collections.Generic;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.ConcentrationModels {
    public sealed class ConcentrationModelsActionResult : IActionResult {
        public IDictionary<(Food, Compound), CompoundResidueCollection> CompoundResidueCollections { get; set; }
        public IDictionary<(Food, Compound), ConcentrationModel> ConcentrationModels { get; set; }
        public IDictionary<Food, ConcentrationModel> CumulativeConcentrationModels { get; set; }
        public ICollection<MarginalOccurrencePattern> SimulatedOccurrencePatterns { get; set; } 
        public ICollection<SampleCompoundCollection> MonteCarloSubstanceSampleCollections { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
