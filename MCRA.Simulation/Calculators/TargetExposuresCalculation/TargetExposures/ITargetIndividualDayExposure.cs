namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualDayExposure : ITargetIndividualExposure {
        string Day { get; }
        int SimulatedIndividualDayId { get; }
    }
}
