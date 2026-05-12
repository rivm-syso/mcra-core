namespace MCRA.Simulation.Calculators.Stratification {

    public interface ICompositeStratificationLevel : IStratificationLevel {

        List<IStratificationLevel> Parts { get; }
    }
}
