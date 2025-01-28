using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels {
    public interface IDistributionProcessingFactorModel {
        ProcessingDistributionType DistributionType { get; }
        double Mu { get; }
        double Sigma { get; }
    }
}