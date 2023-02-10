using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;

namespace MCRA.Simulation.Actions.SingleValueRisks {
    public class SingleValueRisksOutputData : IModuleOutputData {
        public ICollection<SingleValueRiskCalculationResult> SingleValueRiskCalculationResults { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueRisksOutputData() {
                SingleValueRiskCalculationResults = SingleValueRiskCalculationResults
            };
        }
    }
}

