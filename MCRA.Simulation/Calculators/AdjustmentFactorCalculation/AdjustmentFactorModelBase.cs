using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public abstract class AdjustmentFactorModelBase : IAdjustmentFactorModel {
        public AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod { get; set; }
        public abstract double DrawFromDistribution(IRandom random);
        public abstract double GetNominal();
    }
}
