
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Foods {
    public class FoodsOutputData : IModuleOutputData {
        public ICollection<Food> AllFoods { get; set; }
        public IDictionary<string, Food> AllFoodsByCode { get; set; }
        public ICollection<ProcessingType> ProcessingTypes { get; set; }
        public IModuleOutputData Copy() {
            return new FoodsOutputData() {
                AllFoods = AllFoods,
                AllFoodsByCode = AllFoodsByCode,
                ProcessingTypes = ProcessingTypes
            };
        }
    }
}

