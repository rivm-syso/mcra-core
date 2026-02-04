
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {
    public class DustConcentrationDistributionsOutputData : IModuleOutputData {
        public IList<DustConcentrationDistribution> DustConcentrationDistributions { get; set; }
        public IDictionary<Compound, ConcentrationModel> DustConcentrationModels { get; set; }
        public ConcentrationUnit DustConcentrationDistributionUnit { get; set; }
        public IModuleOutputData Copy() {
            return new DustConcentrationDistributionsOutputData() {
                DustConcentrationDistributions = DustConcentrationDistributions,
                DustConcentrationModels = DustConcentrationModels,
                DustConcentrationDistributionUnit = DustConcentrationDistributionUnit
            };
        }
    }
}
