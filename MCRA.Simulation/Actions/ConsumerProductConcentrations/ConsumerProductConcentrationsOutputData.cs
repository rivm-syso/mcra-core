
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations
 {
    public class ConsumerProductConcentrationsOutputData : IModuleOutputData {
        public ICollection<ConsumerProductConcentration> AllConsumerProductConcentrations { get; set; }
        public ConcentrationUnit ConsumerProductConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductConcentrationsOutputData() {
                AllConsumerProductConcentrations = AllConsumerProductConcentrations,
                ConsumerProductConcentrationUnit = ConsumerProductConcentrationUnit
            };
        }
    }
}

