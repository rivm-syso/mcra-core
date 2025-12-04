using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SimulatedPopulations {
    public interface IPopulationSizeModel {
        double Draw(IRandom random);
    }
}