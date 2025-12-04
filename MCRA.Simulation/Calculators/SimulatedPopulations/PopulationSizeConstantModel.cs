using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SimulatedPopulations {

    public class PopulationSizeConstantModel : IPopulationSizeModel {

        public double Constant { get; protected set; }

        public PopulationSizeConstantModel(double value) {
            Constant = value;
        }

        public double Draw(IRandom random) {
            return Constant;
        }
    }
}
