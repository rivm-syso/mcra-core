using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public interface IHbmNonDetectImputationConcentrationCalculator {

        (List<HumanMonitoringSampleSubstanceCollection>, IDictionary<Compound, ConcentrationModel>) ImputeNonDetects(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            IRandom random
        );
    }
}
