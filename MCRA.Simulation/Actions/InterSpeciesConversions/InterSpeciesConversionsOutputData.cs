
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.InterSpeciesConversion;

namespace MCRA.Simulation.Actions.InterSpeciesConversions {
    public class InterSpeciesConversionsOutputData : IModuleOutputData {
        public ICollection<InterSpeciesFactor> InterSpeciesFactors { get; set; }
        public IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> InterSpeciesFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new InterSpeciesConversionsOutputData() {
                InterSpeciesFactors = InterSpeciesFactors,
                InterSpeciesFactorModels = InterSpeciesFactorModels
            };
        }
    }
}

