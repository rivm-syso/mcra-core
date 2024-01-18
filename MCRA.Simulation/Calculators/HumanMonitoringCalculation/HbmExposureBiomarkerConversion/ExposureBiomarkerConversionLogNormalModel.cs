using MCRA.Data.Compiled.Objects;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {

    public sealed class ExposureBiomarkerConversionLogNormalModel : ExposureBiomarkerConversionModelBase {

        private double _mu;
        private double _sigma;

        public ExposureBiomarkerConversionLogNormalModel(ExposureBiomarkerConversion conversion) : base(conversion) {
        }

        public override void CalculateParameters() {
            _mu = UtilityFunctions.LogBound(ConversionRule.Factor);
            if (!ConversionRule.VariabilityUpper.HasValue) {
                throw new Exception($"Exposure biomarker conversion: missing upper value for distribution [{ConversionRule.Distribution.GetDisplayName()}].");
            }
            var upper = ConversionRule.VariabilityUpper.Value;
            if (!double.IsNaN(upper)) {
                _sigma = (UtilityFunctions.LogBound(upper) - _mu) / 1.645;
            }
        }

        public override double Draw(IRandom random) {
            var factor = UtilityFunctions.ExpBound(NormalDistribution.DrawInvCdf(random, _mu, _sigma));
            return factor;
        }
    }
}
