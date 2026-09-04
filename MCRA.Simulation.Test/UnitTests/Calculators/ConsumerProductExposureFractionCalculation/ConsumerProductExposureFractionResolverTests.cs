using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureFractionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConsumerProductExposureFractionCalculation {

    [TestClass]
    public class ConsumerProductExposureFractionResolverTests {

        [TestMethod]
        [DataRow(null, GenderType.Undefined, 0.9)]
        [DataRow(null, GenderType.Female, 0.2)]
        [DataRow(null, GenderType.Male, 0.3)]
        [DataRow(5d, GenderType.Undefined, 0.4)]
        [DataRow(5d, GenderType.Male, 0.5)]
        [DataRow(5d, GenderType.Female, 0.6)]
        public void ConsumerProductExposureFractionResolver_TestResolve(
            double? age,
            GenderType gender,
            double expected
        ) {
            var product = FakeConsumerProductsGenerator.Create(1)[0];
            var substance = FakeSubstancesGenerator.Create(1)[0];

            var fraction = new ConsumerProductExposureFraction {
                Product = product,
                Substance = substance,
                Route = ExposureRoute.Oral,
                ExposureFraction = expected,
                AgeLower = age,
                Sex = gender
            };

            var exposureFractions = new Dictionary<(ConsumerProduct, Compound, ExposureRoute), ConsumerProductExposureFractionSGs> {
                { (product, substance, ExposureRoute.Oral),
                new ConsumerProductExposureFractionSGs() {
                    Product = product,
                    Substance = substance,
                    Route = ExposureRoute.Oral,
                    CPEFSubgroups = [fraction] }
                }
            };

            var kvp = exposureFractions.First();
            var resolver = new ConsumerProductExposureFractionResolver(kvp.Value);
            var cpfraction = resolver.GetExposureFraction(age, gender);
            var subgroup = kvp.Value.CPEFSubgroups
                .FirstOrDefault(c => c.AgeLower.HasValue && age >= c.AgeLower.Value);
            Assert.AreEqual(expected, cpfraction);
        }

        [TestMethod]
        [DataRow(null, GenderType.Undefined)]
        [DataRow(null, GenderType.Female)]
        [DataRow(null, GenderType.Male)]
        [DataRow(5d, GenderType.Undefined)]
        [DataRow(5d, GenderType.Male)]
        [DataRow(5d, GenderType.Female)]
        public void ConsumerProductExposureFractionResolver_TestGeneric(
            double? age,
            GenderType gender
        ) {
            var product = FakeConsumerProductsGenerator.Create(1)[0];
            var substance = FakeSubstancesGenerator.Create(1)[0];

            var fraction = new ConsumerProductExposureFraction {
                Product = product,
                Substance = substance,
                Route = ExposureRoute.Oral,
                ExposureFraction = .345
            };

            var exposureFractions = new Dictionary<(ConsumerProduct, Compound, ExposureRoute), ConsumerProductExposureFractionSGs> {
                { (product, substance, ExposureRoute.Oral),
                new ConsumerProductExposureFractionSGs() {
                    Product = product,
                    Substance = substance,
                    Route = ExposureRoute.Oral,
                    CPEFSubgroups = [fraction] }
                }
            };

            var kvp = exposureFractions.First();
            var resolver = new ConsumerProductExposureFractionResolver(kvp.Value);
            var cpfraction = resolver.GetExposureFraction(age, gender);
            var subgroup = kvp.Value.CPEFSubgroups
                .FirstOrDefault(c => c.AgeLower.HasValue && age >= c.AgeLower.Value);
            Assert.AreEqual(.345, cpfraction);
        }

        [TestMethod]
        [DataRow(null, GenderType.Undefined, .5)]
        [DataRow(null, GenderType.Female, .5)]
        [DataRow(null, GenderType.Male, .6)]
        [DataRow(4d, GenderType.Undefined, .5)]
        [DataRow(4d, GenderType.Female, .5)]
        [DataRow(5d, GenderType.Male, .7)]
        [DataRow(6d, GenderType.Male, .7)]
        [DataRow(6d, GenderType.Female, .5)]
        public void ConsumerProductExposureFractionResolver_TestGenericSpecific(
            double? age,
            GenderType gender,
            double? expected
        ) {
            var product = FakeConsumerProductsGenerator.Create(1)[0];
            var substance = FakeSubstancesGenerator.Create(1)[0];

            var exposureFractions = new Dictionary<(ConsumerProduct, Compound, ExposureRoute), ConsumerProductExposureFractionSGs> {
                { (product, substance, ExposureRoute.Oral),
                new ConsumerProductExposureFractionSGs() {
                    Product = product,
                    Substance = substance,
                    Route = ExposureRoute.Oral,
                    CPEFSubgroups = [
                        new ConsumerProductExposureFraction {
                            Product = product,
                            Substance = substance,
                            Route = ExposureRoute.Oral,
                            ExposureFraction = .5
                        },
                        new ConsumerProductExposureFraction {
                            Product = product,
                            Substance = substance,
                            Route = ExposureRoute.Oral,
                            ExposureFraction = 0.6,
                            Sex = GenderType.Male
                        },
                        new ConsumerProductExposureFraction {
                            Product = product,
                            Substance = substance,
                            Route = ExposureRoute.Oral,
                            ExposureFraction = 0.7,
                            AgeLower = 5,
                            Sex = GenderType.Male
                        }
                    ]}
                }
            };

            var kvp = exposureFractions.First();
            var resolver = new ConsumerProductExposureFractionResolver(kvp.Value);
            var cpfraction = resolver.GetExposureFraction(age, gender);
            var subgroup = kvp.Value.CPEFSubgroups
                .FirstOrDefault(c => c.AgeLower.HasValue && age >= c.AgeLower.Value);
            Assert.AreEqual(expected, cpfraction);
        }
    }
}
