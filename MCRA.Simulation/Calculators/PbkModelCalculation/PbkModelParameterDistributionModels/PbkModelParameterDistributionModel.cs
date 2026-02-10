using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.PbkModelParameterDistributionModels {
    public abstract class PbkModelParameterDistributionModel {
        public double mu;
        public double sigma;

        public abstract void Initialize(double mean, double? cv);
        public abstract double Sample(IRandom random);
    }
}
