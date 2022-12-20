using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels {
    public abstract class ProbabilityDistributionModel {
        public double mu;
        public double sigma;

        public abstract void Initialize(double mean, double sd);
        public abstract double Sample(IRandom random);
    }
}
