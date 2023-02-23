using MCRA.Utils.Statistics;
using MCRA.General;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public interface IAdjustmentFactorModel {
        AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod { get; set; }
        double DrawFromDistribution(IRandom random);
        double GetNominal();
    }
}
