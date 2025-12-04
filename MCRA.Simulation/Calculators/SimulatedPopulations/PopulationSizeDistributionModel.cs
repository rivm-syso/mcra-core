using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SimulatedPopulations {

    public class PopulationSizeDistributionModel<T> : IPopulationSizeModel where T : Distribution {

        public T Distribution { get; protected set; }

        public PopulationSizeDistributionModel(T distribution) {
            Distribution = distribution;
        }

        public double Draw(IRandom random) {
            return Distribution.Draw(random);
        }
    }
}
