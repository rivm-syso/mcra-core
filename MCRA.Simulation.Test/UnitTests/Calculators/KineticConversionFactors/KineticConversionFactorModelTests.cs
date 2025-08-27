using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticConversionFactors {
    [TestClass]
    public class KineticConversionFactorModelTests {

        [TestMethod]
        [DataRow(KineticConversionFactorDistributionType.Unspecified, true)]
        [DataRow(KineticConversionFactorDistributionType.LogNormal, true)]
        [DataRow(KineticConversionFactorDistributionType.Uniform, true)]
        [DataRow(KineticConversionFactorDistributionType.Unspecified, false)]
        [DataRow(KineticConversionFactorDistributionType.LogNormal, false)]
        [DataRow(KineticConversionFactorDistributionType.Uniform, false)]
        public void KineticConversionFactorModel_TestCreate_NoSubGroups(
            KineticConversionFactorDistributionType distributionType,
            bool isNominal
        ) {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                UncertaintyUpper = 0.6,
                Distribution = distributionType,
            };
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, false);
            Assert.IsNotNull(model);

            if (!isNominal) {
                var random = new McraRandomGenerator(1);
                model.ResampleModelParameters(random);
            }

            var draw = model.GetConversionFactor(75, GenderType.Male);
            if (isNominal || distributionType == KineticConversionFactorDistributionType.Unspecified) {
                Assert.AreEqual(0.4, draw);
            } else {
                // This is an uncertainty draw; it is be very suspicious when it is
                // exactly the same as the nominal value
                Assert.AreNotEqual(0.4, draw);
                Assert.IsTrue(draw > 0);
            }
        }

        [TestMethod]
        [DataRow(KineticConversionFactorDistributionType.Unspecified, true)]
        [DataRow(KineticConversionFactorDistributionType.LogNormal, true)]
        [DataRow(KineticConversionFactorDistributionType.Uniform, true)]
        [DataRow(KineticConversionFactorDistributionType.Unspecified, false)]
        [DataRow(KineticConversionFactorDistributionType.LogNormal, false)]
        [DataRow(KineticConversionFactorDistributionType.Uniform, false)]
        public void KineticConversionFactorModel_TestCreate_SubGroupsAgeGender(
            KineticConversionFactorDistributionType distributionType,
            bool isNominal
        ) {
            var nominalFactor = 0.5;
            var subgroups = new List<(double? age, GenderType gender, double factor, double? uncertaintyUpper)>() {
                (0, GenderType.Male, 1.0, 1.5),
                (0, GenderType.Female, 2.0, 2.5),
                (10, GenderType.Male, 1.1, 1.6),
                (10, GenderType.Female, 2.1, 2.6),
                (null, GenderType.Male, 1.5, 1.9),
                (null, GenderType.Female, 2.5, 2.9)
            };
            var conversion = createKineticConversionFactor(distributionType, subgroups, nominalFactor);
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, true);
            Assert.IsNotNull(model);

            if (!isNominal) {
                var random = new McraRandomGenerator(1);
                model.ResampleModelParameters(random);
            }

            var scenarios = new List<(double? age, GenderType gender, double expectedConst)>() {
                (5, GenderType.Male, 1.0),
                (10, GenderType.Male, 1.1),
                (11, GenderType.Male, 1.1),
                (5, GenderType.Female, 2.0),
                (10, GenderType.Female, 2.1),
                (11, GenderType.Undefined, 0.5),
                (null, GenderType.Male, 1.5),
                (null, GenderType.Female, 2.5),
                (null, GenderType.Undefined, 0.5)
            };
            foreach (var scenario in scenarios) {
                var draw = model.GetConversionFactor(scenario.age, scenario.gender);
                if (isNominal || distributionType == KineticConversionFactorDistributionType.Unspecified) {
                    Assert.AreEqual(scenario.expectedConst, draw);
                } else {
                    // This is an uncertainty draw; it is be very suspicious when it is
                    // exactly the same as the nominal value
                    Assert.AreNotEqual(scenario.expectedConst, draw);
                    Assert.IsTrue(draw > 0);
                }
            }
        }

        /// <summary>
        /// Throw exception when no uncertainyUpper is specified.
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorModel_TestsLognormalUncertain_FailNoUpper() {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                Distribution = KineticConversionFactorDistributionType.LogNormal,
            };
            Assert.ThrowsExactly<Exception>(() => KineticConversionFactorCalculatorFactory.Create(conversion, false));
        }

        /// <summary>
        /// Throw exception when no uncertainyUpper is specified for sub-group.
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorModel_TestLognormalUncertain_FailNoSgUpper() {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                UncertaintyUpper = 0.1,
                Distribution = KineticConversionFactorDistributionType.LogNormal,
                KCFSubgroups = [
                    new KineticConversionFactorSG() {
                        ConversionFactor = 0.1,
                        AgeLower = 0
                    }
                ]
            };
            Assert.ThrowsExactly<Exception>(() => KineticConversionFactorCalculatorFactory.Create(conversion, true));
        }

        [TestMethod]
        public void KineticConversionFactorModel_TestInverseUniform() {
            var upper = 60;
            var nominal = 30;
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = nominal,
                UncertaintyUpper = upper,
                Distribution = KineticConversionFactorDistributionType.InverseUniform,
            };
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, false);
            Assert.IsNotNull(model);

            var n = 100;

            var draws = new List<double>();
            var random = new McraRandomGenerator(1);
            for (int i = 0; i < n; i++) {
                model.ResampleModelParameters(random);
                var draw = model.GetConversionFactor(40, GenderType.Male);
                draws.Add(draw);
            }

            var min = draws.Min();
            var max = draws.Max();
            Assert.IsTrue(min < nominal);
            Assert.IsTrue(max > nominal);
        }

        private static KineticConversionFactor createKineticConversionFactor(
            KineticConversionFactorDistributionType distributionType,
            List<(double? age, GenderType gender, double factor, double? uncertaintyUpper)> subgroups,
            double nominalFactor
        ) {
            return new KineticConversionFactor() {
                ConversionFactor = nominalFactor,
                Distribution = distributionType,
                UncertaintyUpper = nominalFactor * 1.1,
                KCFSubgroups = subgroups?
                    .Select(r => new KineticConversionFactorSG() {
                        ConversionFactor = r.factor,
                        AgeLower = r.age,
                        Gender = r.gender,
                        UncertaintyUpper = r.uncertaintyUpper
                    })
                    .ToList() ?? []
            };
        }
    }
}
