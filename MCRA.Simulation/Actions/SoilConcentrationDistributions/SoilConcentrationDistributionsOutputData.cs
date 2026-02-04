
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {
    public class SoilConcentrationDistributionsOutputData : IModuleOutputData {
        public IList<SoilConcentrationDistribution> SoilConcentrationDistributions { get; set; }
        public IDictionary<Compound, ConcentrationModel> SoilConcentrationModels { get; set; }
        public ConcentrationUnit SoilConcentrationDistributionUnit { get; set; }
        public IModuleOutputData Copy() {
            return new SoilConcentrationDistributionsOutputData() {
                SoilConcentrationDistributions = SoilConcentrationDistributions,
                SoilConcentrationDistributionUnit = SoilConcentrationDistributionUnit,
                SoilConcentrationModels = SoilConcentrationModels
            };
        }
    }
}
