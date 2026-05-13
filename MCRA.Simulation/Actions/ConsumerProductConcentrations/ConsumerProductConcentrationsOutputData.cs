using MCRA.Simulation.Action;
using MCRA.Simulation.Objects.SubstanceConcentrations;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations {
    public class ConsumerProductConcentrationsOutputData : IModuleOutputData {
        public ICollection<ConsumerProductSubstanceConcentrationsCollection> AllConsumerProductConcentrationsCollections { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductConcentrationsOutputData() {
                AllConsumerProductConcentrationsCollections = AllConsumerProductConcentrationsCollections
            };
        }
    }
}

