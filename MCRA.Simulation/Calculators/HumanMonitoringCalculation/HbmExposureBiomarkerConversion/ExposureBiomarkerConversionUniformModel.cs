using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionUniformModel : ExposureBiomarkerConversionModelBase {

        private double _upper;
        private double _lower;

        public ExposureBiomarkerConversionUniformModel(ExposureBiomarkerConversion conversion) : base(conversion) {
        }

        public override void CalculateParameters() {
            var factor = ConversionRule.Factor;
            if (!ConversionRule.VariabilityUpper.HasValue) {
                throw new Exception($"Exposure biomarker conversion: missing upper value for distribution [{ConversionRule.Distribution.GetDisplayName()}].");
            }
            _upper = ConversionRule.VariabilityUpper.Value;
            if (factor > _upper) {
                throw new Exception($"Exposure biomarker conversion: the conversion factor ({factor}) should be smaller than the upper value ({_upper}).");
            }
            var range = _upper - factor;
            if (range > 0.5) {
                throw new Exception($"Exposure biomarker conversion: the difference between the conversion factor ({factor}) and the upper value ({_upper}) should be smaller than 0.5.");
            }
            _lower = factor - range;
            if (!double.IsNaN(_upper) && _upper > 1D ) {
                throw new Exception($"Exposure biomarker conversion: the maximum uncertainty upper value for the uniform distribution is 1, the specified value = {_upper}. ");
            }
        }

        public override double Draw(IRandom random) {
            return random.NextDouble(_lower, _upper);
        }
    }
}
