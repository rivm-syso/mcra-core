using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public class KineticConversionFactorCalculatorFactory {

        public static KineticConversionFactorModelBase Create(
            KineticConversionFactor conversion
        ) {
            KineticConversionFactorModelBase model = null;
            if (!conversion.UncertaintyUpper.HasValue) {
                return new KineticConversionFactorConstantModel(conversion);
            }
            switch (conversion.Distribution) {
                case BiomarkerConversionDistribution.Unspecified:
                    model = new KineticConversionFactorConstantModel(conversion);
                    break;
                case BiomarkerConversionDistribution.LogNormal:
                    model = new KineticConversionFactorLogNormalModel(conversion);
                    break;
                case BiomarkerConversionDistribution.Uniform:
                    model = new KineticConversionFactorUniformModel(conversion);
                    break;
            }
            model.CalculateParameters();
            return model;
        }
    }
}
