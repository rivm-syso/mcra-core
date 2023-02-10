using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.Actions.Risks {
    public class RisksOutputData : IModuleOutputData {
        public ICollection<IndividualEffect> CumulativeIndividualEffects { get; set; }
        public IModuleOutputData Copy() {
            return new RisksOutputData() {
                CumulativeIndividualEffects = CumulativeIndividualEffects,
            };
        }
    }
}

