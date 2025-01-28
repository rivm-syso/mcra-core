using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;

namespace MCRA.Simulation.Actions.ProcessingFactors {
    public class ProcessingFactorsOutputData : IModuleOutputData {
        public ICollection<ProcessingFactor> ProcessingFactors { get; set; }
        public ICollection<ProcessingFactorModel> ProcessingFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new ProcessingFactorsOutputData() {
                ProcessingFactors = ProcessingFactors,
                ProcessingFactorModels = ProcessingFactorModels
            };
        }
    }
}

