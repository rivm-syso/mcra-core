
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrationDistributions {
    public class ConsumerProductConcentrationDistributionsOutputData : IModuleOutputData {
        public ICollection<ConsumerProductConcentrationDistribution> AllConsumerProductConcentrationDistributions { get; set; }
        public IDictionary<(ConsumerProduct, Compound), ConcentrationModel> ConsumerProductConcentrationModels { get; set; }
        public ConcentrationUnit ConsumerProductConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductConcentrationDistributionsOutputData() {
                AllConsumerProductConcentrationDistributions = AllConsumerProductConcentrationDistributions,
                ConsumerProductConcentrationUnit = ConsumerProductConcentrationUnit,
                ConsumerProductConcentrationModels = ConsumerProductConcentrationModels
            };
        }
    }
}

