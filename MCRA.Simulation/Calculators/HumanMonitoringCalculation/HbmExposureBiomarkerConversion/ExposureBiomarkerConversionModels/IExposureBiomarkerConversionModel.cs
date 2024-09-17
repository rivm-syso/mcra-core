using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels {
    public interface IExposureBiomarkerConversionModel {
        public ExposureBiomarkerConversion ConversionRule { get; }
        public double Draw(IRandom random, double? age, GenderType gender);
        public void CalculateParameters();
    }
}
