using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public sealed class KineticConversionFactorUniformModel : KineticConversionFactorModelBase {

        private double _upper;
        private double _lower;

        public KineticConversionFactorUniformModel(KineticConversionFactor conversion) : base(conversion) {
        }

        public override void CalculateParameters() {
            var factor = ConversionRule.ConversionFactor;
            if (!ConversionRule.UncertaintyUpper.HasValue) {
                throw new Exception($"Exposure biomarker conversion: missing upper value for distribution [{ConversionRule.Distribution.GetDisplayName()}].");
            }
            _upper = ConversionRule.UncertaintyUpper.Value;
            if (factor > _upper) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor ({factor}) should be smaller than the upper value ({_upper}).");
            }
            var range = _upper - factor;
            _lower = factor - range;
            if (_lower < 0) {
                throw new Exception($"Exposure biomarker conversion: the difference between the conversion factor ({factor}) and the upper value ({_upper}) = {range}, and should be smaller than {factor}.");
            }
        }

        public override double Draw(IRandom random) {
            return random.NextDouble(_lower, _upper);
        }
    }
}
