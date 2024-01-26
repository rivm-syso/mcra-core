using MCRA.Data.Compiled.Objects;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {

    public sealed class KineticConversionFactorLogNormalModel : KineticConversionFactorModelBase {

        private double _mu;
        private double _sigma;

        public KineticConversionFactorLogNormalModel(KineticConversionFactor conversion) 
            : base(conversion) {
        }
        public override void CalculateParameters() {
            _mu = UtilityFunctions.LogBound(ConversionRule.ConversionFactor);
            if (!ConversionRule.UncertaintyUpper.HasValue) {
                throw new Exception($"Exposure biomarker conversion: missing upper value for distribution [{ConversionRule.Distribution.GetDisplayName()}].");
            }
            var upper = ConversionRule.UncertaintyUpper.Value;
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
