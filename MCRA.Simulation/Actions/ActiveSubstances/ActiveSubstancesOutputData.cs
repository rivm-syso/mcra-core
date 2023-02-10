using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ActiveSubstances {
    public class ActiveSubstancesOutputData : IModuleOutputData {
        public ICollection<ActiveSubstanceModel> AvailableActiveSubstanceModels { get; set; }
        public IDictionary<Compound, double> MembershipProbabilities { get; set; }
        public ICollection<Compound> ActiveSubstances { get; set; }

        public IModuleOutputData Copy() {
            return new ActiveSubstancesOutputData() {
                AvailableActiveSubstanceModels = AvailableActiveSubstanceModels,
                MembershipProbabilities = MembershipProbabilities,
                ActiveSubstances = ActiveSubstances
            };
        }
    }
}
