using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.ActiveSubstances {
    public class ActiveSubstancesActionResult : IActionResult {
        public ICollection<Compound> ActiveSubstances { get; set; }
        public ActiveSubstanceModel ActiveSubstanceModel { get; set; }
        public ICollection<ActiveSubstanceModel> AvailableActiveSubstanceModels { get; set; }
        public ICollection<ActiveSubstanceModel> AopNetworkEffectsActiveSubstanceModels { get; set; }
        public IDictionary<Compound, double> MembershipProbabilities { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
