
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {
    public class IntraSpeciesFactorsOutputData : IModuleOutputData {
        public ICollection<IntraSpeciesFactor> IntraSpeciesFactors { get; set; }
        public IDictionary<(Effect, Compound), IntraSpeciesFactorModel> IntraSpeciesFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new IntraSpeciesFactorsOutputData() {
                IntraSpeciesFactors = IntraSpeciesFactors,
                IntraSpeciesFactorModels = IntraSpeciesFactorModels
            };
        }
    }
}

