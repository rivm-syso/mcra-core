using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ProcessingFactorCalculation {
    public interface IDistributionProcessingFactorModel {
        Food Food { get; set; }
        ProcessingType ProcessingType { get; set; }
        Compound Substance { get; set; }
        ProcessingDistributionType DistributionType { get; }
        double Mu { get; }
        double Sigma { get; }
    }
}