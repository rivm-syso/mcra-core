using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public abstract class ExposureBiomarkerConversionModelBase {

        public ExposureBiomarkerConversion ConversionRule { get; protected set; }

        public ExposureBiomarkerConversionModelBase(
            ExposureBiomarkerConversion conversion
        ) {
            ConversionRule = conversion;
        }

        public abstract void CalculateParameters();
        public abstract double Draw(IRandom random);
    }
}
