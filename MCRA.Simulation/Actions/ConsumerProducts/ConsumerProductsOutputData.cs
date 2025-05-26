using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumerProducts {
    public class ConsumerProductsOutputData : IModuleOutputData {
        public ICollection<ConsumerProduct> AllConsumerProducts { get; set; }
        public IDictionary<string, ConsumerProduct> AllConsumerProductsByCode { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductsOutputData() {
                AllConsumerProducts = AllConsumerProducts,
                AllConsumerProductsByCode = AllConsumerProductsByCode,
            };
        }
    }
}

