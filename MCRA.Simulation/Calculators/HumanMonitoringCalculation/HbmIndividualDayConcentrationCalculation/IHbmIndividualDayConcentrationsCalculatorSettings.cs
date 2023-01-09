using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public interface IHbmIndividualDayConcentrationsCalculatorSettings {

        NonDetectsHandlingMethod NonDetectsHandlingMethod { get; }
        double LorReplacementFactor { get; }
        MissingValueImputationMethod MissingValueImputationMethod { get; }
        NonDetectImputationMethod NonDetectImputationMethod { get; }

    }
}
