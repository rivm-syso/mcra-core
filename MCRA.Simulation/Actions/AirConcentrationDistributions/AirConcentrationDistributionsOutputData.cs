
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.AirConcentrationDistributions {
    public class AirConcentrationDistributionsOutputData : IModuleOutputData {
        public IList<AirConcentrationDistribution> AirConcentrationDistributions { get; set; }
        public IDictionary<Compound, ConcentrationModel> IndoorAirConcentrationModels { get; set; }
        public IDictionary<Compound, ConcentrationModel> OutdoorAirConcentrationModels { get; set; }
        public AirConcentrationUnit AirConcentrationDistributionUnit { get; set; }
        public IModuleOutputData Copy() {
            return new AirConcentrationDistributionsOutputData() {
                AirConcentrationDistributions = AirConcentrationDistributions,
                IndoorAirConcentrationModels = IndoorAirConcentrationModels,
                OutdoorAirConcentrationModels = OutdoorAirConcentrationModels,
                AirConcentrationDistributionUnit = AirConcentrationDistributionUnit,
            };
        }
    }
}
