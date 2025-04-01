
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.UnitVariabilityFactors {
    public class UnitVariabilityFactorsOutputData : IModuleOutputData {
        public Dictionary<Food, FoodUnitVariabilityInfo> UnitVariabilityDictionary { get; set; }
        public ICollection<IestiSpecialCase> IestiSpecialCases { get; set; }
        public IModuleOutputData Copy() {
            return new UnitVariabilityFactorsOutputData() {
                UnitVariabilityDictionary = UnitVariabilityDictionary,
                IestiSpecialCases = IestiSpecialCases
            };
        }
    }
}

