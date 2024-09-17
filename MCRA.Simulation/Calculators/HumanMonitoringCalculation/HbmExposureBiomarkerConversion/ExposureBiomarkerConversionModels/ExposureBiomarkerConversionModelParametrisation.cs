using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels {
    public class ExposureBiomarkerConversionModelParametrisation {
        public double Factor { get; set; }
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
    }
}
