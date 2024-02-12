using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactorCalculation {
    [TestClass]
    public class HbmKineticConversionFactorCalculatorTests {

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void HbmKineticConversionFactorCalculator_TestConstNoUnc(bool isNominal) {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                Distribution = BiomarkerConversionDistribution.Unspecified,
            };
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, false, !isNominal);
            Assert.IsNotNull(model);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draw = model.Draw(random, 75, GenderType.Male);
            Assert.AreEqual(0.4, draw);
        }

        [TestMethod]
        [DataRow(BiomarkerConversionDistribution.Unspecified, true)]
        [DataRow(BiomarkerConversionDistribution.LogNormal, true)]
        [DataRow(BiomarkerConversionDistribution.Uniform, true)]
        [DataRow(BiomarkerConversionDistribution.Unspecified, false)]
        [DataRow(BiomarkerConversionDistribution.LogNormal, false)]
        [DataRow(BiomarkerConversionDistribution.Uniform, false)]
        public void HbmKineticConversionFactorCalculator_TestNoSubGroups(
            BiomarkerConversionDistribution distributionType,
            bool isNominal
        ) {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                UncertaintyUpper = 0.6,
                Distribution = distributionType,
            };
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, false, !isNominal);
            Assert.IsNotNull(model);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draw = model.Draw(random, 75, GenderType.Male);
            if (isNominal || distributionType == BiomarkerConversionDistribution.Unspecified) {
                Assert.AreEqual(0.4, draw);
            } else {
                Assert.IsTrue(draw > 0);
            }
        }

        [TestMethod]
        [DataRow(BiomarkerConversionDistribution.Unspecified, true)]
        [DataRow(BiomarkerConversionDistribution.LogNormal, true)]
        [DataRow(BiomarkerConversionDistribution.Uniform, true)]
        [DataRow(BiomarkerConversionDistribution.Unspecified, false)]
        [DataRow(BiomarkerConversionDistribution.LogNormal, false)]
        [DataRow(BiomarkerConversionDistribution.Uniform, false)]
        public void HbmKineticConversionFactorCalculator_TestSubGroupsAgeGender(
            BiomarkerConversionDistribution distributionType,
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
            var model = KineticConversionFactorCalculatorFactory.Create(conversion, true, !isNominal);
            Assert.IsNotNull(model);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
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
                var draw = model.Draw(random, scenario.age, scenario.gender);
                if (isNominal || distributionType == BiomarkerConversionDistribution.Unspecified) {
                    Assert.AreEqual(scenario.expectedConst, draw);
                } else {
                    Assert.IsTrue(draw > 0);
                }
            }
        }

        private static KineticConversionFactor createKineticConversionFactor(
            BiomarkerConversionDistribution distributionType,
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
                    .ToList() ?? new()
            };
        }

        /// <summary>
        /// Throw exception when no uncertainyUpper is specified.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void HbmKineticConversionFactorCalculatorLognormal_TestUncertain_FailNoUpper() {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                Distribution = BiomarkerConversionDistribution.LogNormal,
            };
            _ = KineticConversionFactorCalculatorFactory.Create(conversion, false, true);
        }

        /// <summary>
        /// Throw exception when no uncertainyUpper is specified for sub-group.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void HbmKineticConversionFactorCalculatorLognormal_TestUncertain_FailNoSgUpper() {
            var conversion = new KineticConversionFactor() {
                IdKineticConversionFactor = "id1",
                ConversionFactor = .4,
                UncertaintyUpper = 0.1,
                Distribution = BiomarkerConversionDistribution.LogNormal,
                KCFSubgroups = new List<KineticConversionFactorSG>() {
                    new KineticConversionFactorSG() {
                        ConversionFactor = 0.1,
                        AgeLower = 0
                    }
                }
            };
            _ = KineticConversionFactorCalculatorFactory.Create(conversion, true, true);
        }
    }
}
