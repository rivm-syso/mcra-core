using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public class KineticConversionFactorsOutputData : IModuleOutputData {
        public ICollection<IKineticConversionFactorModel> KineticConversionFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new KineticConversionFactorsOutputData() {
                KineticConversionFactorModels = KineticConversionFactorModels
            };
        }
    }
}

