using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public class KineticConversionFactorCalculatorFactory {

        public static KineticConversionFactorModelBase Create(
            KineticConversionFactor conversion,
            bool useSubgroups, 
            bool isUncertainty
        ) {
            KineticConversionFactorModelBase model = null;

            //In the nominal run, always return Constant Model (e.g. the conversion factor is returned).
            if (!isUncertainty) {
                model = new KineticConversionFactorConstantModel(conversion, useSubgroups);
                model.CalculateParameters();
                return model;
            }

            // In the uncertainty runs, create model for specified uncertainty distribution type
            switch (conversion.Distribution) {
                case BiomarkerConversionDistribution.Unspecified:
                    model = new KineticConversionFactorConstantModel(conversion, useSubgroups);
                    break;
                case BiomarkerConversionDistribution.LogNormal:
                    model = new KineticConversionFactorLogNormalModel(conversion, useSubgroups);
                    break;
                case BiomarkerConversionDistribution.Uniform:
                    model = new KineticConversionFactorUniformModel(conversion, useSubgroups);
                    break;
            }
            model.CalculateParameters();
            return model;
        }
    }
}
