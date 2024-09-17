using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public class KineticConversionFactorCalculatorFactory {

        public static IKineticConversionFactorModel Create(
            KineticConversionFactor conversionFactor,
            bool useSubgroups
        ) {
            IKineticConversionFactorModel model;
            switch (conversionFactor.Distribution) {
                case KineticConversionFactorDistributionType.Unspecified:
                    model = new KineticConversionFactorConstantModel(conversionFactor, useSubgroups);
                    break;
                case KineticConversionFactorDistributionType.LogNormal:
                    model = new KineticConversionFactorLogNormalModel(conversionFactor, useSubgroups);
                    break;
                case KineticConversionFactorDistributionType.Uniform:
                    model = new KineticConversionFactorUniformModel(conversionFactor, useSubgroups);
                    break;
                case KineticConversionFactorDistributionType.InverseUniform:
                    model = new KineticConversionFactorInverseUniformModel(conversionFactor, useSubgroups);
                    break;
                default:
                    var msg = $"No kinetic conversion model for distribution type ${conversionFactor.Distribution}.";
                    throw new NotImplementedException(msg);
            }
            model.CalculateParameters();
            return model;
        }
    }
}
