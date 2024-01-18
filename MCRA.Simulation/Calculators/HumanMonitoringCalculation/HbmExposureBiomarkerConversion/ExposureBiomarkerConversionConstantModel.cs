using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public sealed class ExposureBiomarkerConversionConstantModel : ExposureBiomarkerConversionModelBase{

        public ExposureBiomarkerConversionConstantModel(ExposureBiomarkerConversion conversion) : base(conversion) {
        }

        public override void CalculateParameters() {
        }

        public override double Draw(IRandom random) {
            return ConversionRule.Factor;
        }
    }
}
