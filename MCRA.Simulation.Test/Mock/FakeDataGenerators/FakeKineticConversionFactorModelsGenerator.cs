using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake kinetic conversion models.
    /// </summary>
    public static class FakeKineticConversionFactorModelsGenerator {

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static List<KineticConversionFactor> CreateKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            TargetUnit target
        ) {
            var kineticConversionFactors = new List<KineticConversionFactor>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    kineticConversionFactors.Add(new KineticConversionFactor() {
                        IdKineticConversionFactor = $"KCF_{substance.Code}_{route}_{target.BiologicalMatrix}",
                        SubstanceFrom = substance,
                        SubstanceTo = substance,
                        ExposureRouteFrom = route,
                        DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        ConversionFactor = .5,
                        BiologicalMatrixTo = target.BiologicalMatrix,
                        ExpressionTypeTo = target.ExpressionType,
                        DoseUnitTo = target.ExposureUnit,
                    });
                }
            }
            return kineticConversionFactors;
        }

        public static List<IKineticConversionFactorModel> CreateKineticConversionFactorModels(
            List<Compound> substances,
            List<ExposureRoute> routes,
            TargetUnit target
        ) {
            var kineticConversionFactors = CreateKineticConversionFactors(
                substances,
                routes,
                target
            );
            var kineticConversionFactorModels = kineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))
                .ToList();
            return kineticConversionFactorModels;
        }
    }
}
