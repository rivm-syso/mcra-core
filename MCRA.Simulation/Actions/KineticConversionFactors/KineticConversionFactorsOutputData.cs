using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public class KineticConversionFactorsOutputData : IModuleOutputData {
        public ICollection<KineticConversionFactor> KineticConversionFactors { get; set; }
        public ICollection<KineticConversionFactorModel> KineticConversionFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new KineticConversionFactorsOutputData() {
                KineticConversionFactors = KineticConversionFactors,
                KineticConversionFactorModels = KineticConversionFactorModels
            };
        }
    }
}

