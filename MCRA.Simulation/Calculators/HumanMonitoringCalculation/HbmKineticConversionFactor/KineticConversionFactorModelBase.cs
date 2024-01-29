using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public abstract class KineticConversionFactorModelBase {

        public KineticConversionFactor ConversionRule { get; protected set; }

        public KineticConversionFactorModelBase(KineticConversionFactor conversion) {
            ConversionRule = conversion;
        }

        public virtual void CalculateParameters() { }

        public abstract double Draw(IRandom random);
    }
}
