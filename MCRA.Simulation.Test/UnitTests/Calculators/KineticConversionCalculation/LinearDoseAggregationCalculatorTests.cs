using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticConversionCalculation {

    [TestClass]
    public class LinearDoseAggregationCalculatorTests {

        /// <summary>
        ///  Linear dose aggregation: calculates individual target exposures based on absorption factors.
        /// </summary>
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestAbsorptionFactorModel() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Dermal, ExposureRoute.Oral };
            var paths = FakeExposurePathGenerator.Create(routes);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var kineticConversionFactors = FakeKineticModelsGenerator.CreateKineticConversionFactors(substances, routes, targetUnit);
            var conversionModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                )
                .ToList();
            var calculator = new LinearDoseAggregationCalculator(substance, conversionModels);
            var externalExposures = FakeExternalExposureGenerator
                .CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
            var result = calculator
                .CalculateIndividualDayTargetExposures(
                    externalExposures,
                    routes,
                    externalExposuresUnit,
                    [targetUnit],
                    new ProgressState(),
                    random
                );
            Assert.AreEqual(result.Count, individualDays.Count);
        }

        /// <summary>
        /// Linear dose: calculates reverse dose based on absorption factors.
        /// </summary>
        [DataRow(10, DoseUnit.ugPerL, ExposureRoute.Oral, 1, 10)]
        [DataRow(10, DoseUnit.mgPerL, ExposureRoute.Oral, 1, 10000)]
        [DataRow(10, DoseUnit.ugPerL, ExposureRoute.Dermal, 0.1, 100)]
        [DataRow(10, DoseUnit.ugPerL, ExposureRoute.Inhalation, 0.1, 100)]
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestReverse(
            double internalDose,
            DoseUnit internalDoseUnit,
            ExposureRoute route,
            double factor,
            double expectedExternalDose
        ) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var internalTarget = ExposureTarget.DefaultInternalExposureTarget;
            var factors = new List<KineticConversionFactor>() {
                {
                    new KineticConversionFactor() {
                        ExposureRouteFrom = route,
                        ConversionFactor = factor,
                        SubstanceFrom = substance,
                        DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerKg),
                        BiologicalMatrixTo = internalTarget.BiologicalMatrix,
                        ExpressionTypeTo = internalTarget.ExpressionType
                    }
                }
            };
            var conversionModels = factors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                )
                .ToList();
            var calculator = new LinearDoseAggregationCalculator(substance, conversionModels);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individual = FakeIndividualsGenerator.CreateSingle();
            var externalDose = calculator
                .Reverse(
                    individual,
                    internalDose,
                    TargetUnit.FromInternalDoseUnit(internalDoseUnit, internalTarget.BiologicalMatrix),
                    route,
                    externalExposuresUnit,
                    ExposureType.Chronic,
                    random
                );

            Assert.AreEqual(expectedExternalDose, externalDose);
        }

        /// <summary>
        /// Linear dose: calculates reverse dose based on absorption factors.
        /// </summary>
        [DataRow(10, ExposureRoute.Oral, ExternalExposureUnit.ugPerKgBWPerDay, 0.1, 1)]
        [DataRow(10, ExposureRoute.Oral, ExternalExposureUnit.mgPerKgBWPerDay, 0.1, 0.001)]
        [DataRow(10, ExposureRoute.Dermal, ExternalExposureUnit.ugPerKgBWPerDay, 0.1, 1)]
        [DataRow(10, ExposureRoute.Inhalation, ExternalExposureUnit.ugPerKgBWPerDay, 0.5, 5)]
        [TestMethod]
        public void LinearDoseAggregationCalculatorTests_TestForward(
            double externalDose,
            ExposureRoute route,
            ExternalExposureUnit externalExposureUnit,
            double factor,
            double expectedInternalDose
        ) {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var factors = new List<KineticConversionFactor>() {
                { new KineticConversionFactor(){
                    ExposureRouteFrom = route,
                    ConversionFactor = factor,
                    SubstanceFrom = substance,
                    ExposureRouteTo = route,
                    DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                    DoseUnitTo = ExposureUnitTriple.FromDoseUnit(DoseUnit.ugPerKg),
                }
            }};
            var conversionModels = factors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                )
                .ToList();
            var calculator = new LinearDoseAggregationCalculator(substance, conversionModels);
            var externalExposuresUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var individual = FakeIndividualsGenerator.CreateSingle();
            var internalDose = calculator
                .Forward(
                    individual,
                    externalDose,
                    route,
                    externalExposuresUnit,
                    TargetUnit.FromExternalExposureUnit(externalExposureUnit, route),
                    ExposureType.Chronic,
                    random
                );

            Assert.AreEqual(expectedInternalDose, internalDose);
        }
    }
}
