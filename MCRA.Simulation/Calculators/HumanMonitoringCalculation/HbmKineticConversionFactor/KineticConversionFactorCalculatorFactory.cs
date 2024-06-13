using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public class KineticConversionFactorCalculatorFactory {

        public static KineticConversionFactorModel Create(
            KineticConversionFactor conversion,
            bool useSubgroups
        ) {
            KineticConversionFactorModel model;

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
                default:
                    throw new NotImplementedException($"No kinetic conversion model for distribution type ${conversion.Distribution}.");
            }
            model.CalculateParameters();
            return model;
        }
    }
}
