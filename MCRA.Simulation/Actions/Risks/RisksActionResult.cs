using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksActionResult : IActionResult {
        public IHazardCharacterisationModel ReferenceDose { get; set; }
        public List<IndividualEffect> CumulativeIndividualEffects { get; set; }
        public Dictionary<Compound, List<IndividualEffect>> IndividualEffectsBySubstance { get; set; }
        public Dictionary<Food, List<IndividualEffect>> IndividualEffectsByModelledFood { get; set; }
        public IDictionary<(Food, Compound), List<IndividualEffect>> IndividualEffectsByModelledFoodSubstance { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
