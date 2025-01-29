using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {
    public class SoilConcentrationDistributionsOutputData : IModuleOutputData {
        public IList<SoilConcentrationDistribution> SoilConcentrationDistributions { get; set; }
        public ConcentrationUnit SoilConcentrationUnit { get; set; }
        public IModuleOutputData Copy() {
            return new SoilConcentrationDistributionsOutputData() {
                SoilConcentrationDistributions = SoilConcentrationDistributions,
                SoilConcentrationUnit = SoilConcentrationUnit
            };
        }
    }
}

