using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation {
    public class KineticConversionCalculatorProvider {

        private readonly IDictionary<Compound, IKineticConversionCalculator> _kineticModelCalculators;

        public KineticConversionCalculatorProvider(
            IDictionary<Compound, IKineticConversionCalculator> kineticModelCalculators
        ) {
            _kineticModelCalculators = kineticModelCalculators;
        }

        public IKineticConversionCalculator GetKineticConversionCalculator(
            Compound substance
        ) {
            if (_kineticModelCalculators.TryGetValue(substance, out var result)) {
                return result;
            } else {
                throw new Exception($"No kinetic conversion model found for substance {substance.Name} [{substance.Code}].");
            }
        }
    }
}
