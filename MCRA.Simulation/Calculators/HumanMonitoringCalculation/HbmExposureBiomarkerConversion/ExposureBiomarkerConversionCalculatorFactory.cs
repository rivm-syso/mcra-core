using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public class ExposureBiomarkerConversionCalculatorFactory {

        public static ExposureBiomarkerConversionModelBase Create(
            ExposureBiomarkerConversion conversion
        ) {
            ExposureBiomarkerConversionModelBase model = null;
            if (conversion.VariabilityUpper.HasValue) {
                return new ExposureBiomarkerConversionConstantModel(conversion);
            }
            switch (conversion.Distribution) {
                case BiomarkerConversionDistribution.Unspecified:
                    model = new ExposureBiomarkerConversionConstantModel(conversion);
                    break;
                case BiomarkerConversionDistribution.LogNormal:
                    model = new ExposureBiomarkerConversionLogNormalModel(conversion);
                    break;
                case BiomarkerConversionDistribution.Uniform:
                    model = new ExposureBiomarkerConversionUniformModel(conversion);
                    break;
            }
            model.CalculateParameters();
            return model;
        }
    }
}
