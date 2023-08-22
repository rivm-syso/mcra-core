namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class UrineCorrectionCalculator {

        public UrineCorrectionCalculator(List<string> substancesExcludedFromStandardisation) {
            SubstancesExcludedFromStandardisation = substancesExcludedFromStandardisation;
        }

        protected List<string> SubstancesExcludedFromStandardisation { get; private set; }
    }
}
