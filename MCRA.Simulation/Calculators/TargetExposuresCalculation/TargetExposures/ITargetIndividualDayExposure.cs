namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public interface ITargetIndividualDayExposure : ITargetExposure {
        string Day { get; }
        double IndividualSamplingWeight { get; }
        int SimulatedIndividualDayId { get; }

        double IntraSpeciesDraw { get; set; }
    }
}
