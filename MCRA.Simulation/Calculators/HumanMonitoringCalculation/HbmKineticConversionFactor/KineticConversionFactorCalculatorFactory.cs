using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public class KineticConversionFactorCalculatorFactory {

        public static KineticConversionFactorModelBase Create(
            KineticConversionFactor conversion,
            bool isUncertainty
        ) {
            KineticConversionFactorModelBase model = null;

            //In the nominal run, always return Constant Model (e.g. the conversion factor is returned).
            if (!isUncertainty) {
                model = new KineticConversionFactorConstantModel(conversion);
                model.CalculateParameters();
                return model;
            }

            // In the uncertainty runs, create model for specified uncertainty distribution type
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
