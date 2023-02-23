using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;

namespace MCRA.Simulation.Actions.SingleValueRisks {
    public sealed class SingleValueRisksActionResult : IActionResult {
        public ICollection<SingleValueRiskCalculationResult> SingleValueRiskEstimates { get; set; }
        public double AdjustmentFactorExposure { get; set; }
        public double AdjustmentFactorHazard { get; set; }
        public double FocalCommodityContribution { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
