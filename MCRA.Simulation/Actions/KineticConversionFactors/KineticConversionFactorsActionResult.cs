using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;

namespace MCRA.Simulation.Actions.KineticConversionFactors {
    public class KineticConversionFactorsActionResult : IActionResult {

        public ExposureUnitTriple ExposureUnit { get; set; }
        public ICollection<IKineticConversionFactorModel> KineticConversionFactorModels { get; set; }
        public List<KineticConversionFactorCalculationResult> KineticConversionFactorCalculationResults { get; set; }

        public IUncertaintyFactorialResult FactorialResult { 
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException(); 
        }
    }
}
