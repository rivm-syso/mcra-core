
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;

namespace MCRA.Simulation.Actions.ProcessingFactors {
    public class ProcessingFactorsOutputData : IModuleOutputData {
        public ICollection<ProcessingFactor> ProcessingFactors { get; set; }
        public ProcessingFactorModelCollection ProcessingFactorModels { get; set; }
        public IDictionary<(Food, Compound, ProcessingType), ProcessingFactor> ProcessingFactorsDictionary { get; set; }
        public IModuleOutputData Copy() {
            return new ProcessingFactorsOutputData() {
                ProcessingFactors = ProcessingFactors,
                ProcessingFactorModels = ProcessingFactorModels,
                ProcessingFactorsDictionary = ProcessingFactorsDictionary
            };
        }
    }
}

