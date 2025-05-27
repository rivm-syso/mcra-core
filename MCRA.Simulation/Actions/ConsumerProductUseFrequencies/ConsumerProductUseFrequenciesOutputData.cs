using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {
    public class ConsumerProductUseFrequenciesOutputData : IModuleOutputData {
        public ICollection<IndividualConsumerProductUseFrequency> AllIndividualConsumerProductUseFrequencies { get; set; }

        public ICollection<ConsumerProductSurvey> ConsumerProductSurveys { get; set; }
        public ICollection<Individual> ConsumerProductIndividuals { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductUseFrequenciesOutputData() {
                AllIndividualConsumerProductUseFrequencies = AllIndividualConsumerProductUseFrequencies,
                ConsumerProductSurveys = ConsumerProductSurveys,
                ConsumerProductIndividuals = ConsumerProductIndividuals
            };
        }
    }
}

