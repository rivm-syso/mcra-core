using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public class KineticConversionFactorsActionResult : IActionResult {

        public ICollection<IKineticConversionFactorModel> KineticConversionFactorModels { get; set; }

        public IUncertaintyFactorialResult FactorialResult { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }
    }
}
