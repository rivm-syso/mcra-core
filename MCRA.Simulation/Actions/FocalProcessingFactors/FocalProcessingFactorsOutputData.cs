using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;

namespace MCRA.Simulation.Actions.FocalProcessingFactors {
    public class FocalProcessingFactorsOutputData : IModuleOutputData {
        public ICollection<ProcessingFactor> FocalProcessingFactors { get; set; }
        public IModuleOutputData Copy() {
            return new FocalProcessingFactorsOutputData() {
                FocalProcessingFactors = FocalProcessingFactors,
            };
        }
    }
}

