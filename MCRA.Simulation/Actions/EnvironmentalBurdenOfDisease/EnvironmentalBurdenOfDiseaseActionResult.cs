using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public class EnvironmentalBurdenOfDiseaseActionResult : IActionResult {
        public List<EnvironmentalBurdenOfDiseaseResultRecord> EnvironmentalBurdenOfDiseases { get; set; }
        public List<ExposureEffectResultRecord> ExposureEffects { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
