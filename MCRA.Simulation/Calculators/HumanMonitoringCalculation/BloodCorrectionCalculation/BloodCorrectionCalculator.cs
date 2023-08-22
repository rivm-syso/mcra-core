namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {
    public class BloodCorrectionCalculator {

        public BloodCorrectionCalculator(List<string> substancesExcludedFromStandardisation) {
            SubstancesExcludedFromStandardisation = substancesExcludedFromStandardisation;
        }

        protected List<string> SubstancesExcludedFromStandardisation { get; private set; }
    }
}
