using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public class KineticConversionFactorsOutputData : IModuleOutputData {
        public ICollection<KineticConversionFactor> KineticConversionFactors { get; set; }
        public ICollection<IKineticConversionFactorModel> KineticConversionFactorModels { get; set; }
        public IModuleOutputData Copy() {
            return new KineticConversionFactorsOutputData() {
                KineticConversionFactors = KineticConversionFactors,
                KineticConversionFactorModels = KineticConversionFactorModels
            };
        }
    }
}

