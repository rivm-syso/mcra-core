using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public sealed class KineticConversionFactorConstantModel : KineticConversionFactorModelBase {

        public KineticConversionFactorConstantModel(KineticConversionFactor conversion) : base(conversion) {
        }

        public override double Draw(IRandom random) {
            return ConversionRule.ConversionFactor;
        }
    }
}
