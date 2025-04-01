
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {
    public class FocalFoodConcentrationsOutputData : IModuleOutputData {
        public ICollection<(Food Food, Compound Substance)> FocalCommodityCombinations { get; set; }
        public ICollection<FoodSample> FocalCommoditySamples { get; set; }
        public ICollection<SampleCompoundCollection> FocalCommoditySubstanceSampleCollections { get; set; }
        public IModuleOutputData Copy() {
            return new FocalFoodConcentrationsOutputData() {
                FocalCommodityCombinations = FocalCommodityCombinations,
                FocalCommoditySamples = FocalCommoditySamples,
                FocalCommoditySubstanceSampleCollections = FocalCommoditySubstanceSampleCollections
            };
        }
    }
}

