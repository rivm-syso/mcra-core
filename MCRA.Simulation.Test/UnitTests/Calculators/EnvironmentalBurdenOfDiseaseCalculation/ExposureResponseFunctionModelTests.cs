using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctionModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    [TestClass]
    public class ExposureResponseFunctionModelTests {

        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeConstant() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = new NCalc.Expression("5.01")
            };
            var erfModel = ExposureResponseModelBuilder.Create(erf);
            var random = new McraRandomGenerator();
            var result = erfModel.Compute(42, true);
            Assert.AreEqual(5.01, result);
        }

        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeFunction() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Function,
                ExposureResponseSpecification = new NCalc.Expression("x * 3"),
                ExposureResponseSpecificationLower = new NCalc.Expression("x * 2"),
                ExposureResponseSpecificationUpper = new NCalc.Expression("x * 4"),
                ERFUncertaintyDistribution = ExposureResponseSpecificationDistributionType.Triangular
            };

            var erfModel = ExposureResponseModelBuilder.Create(erf);
            Assert.AreEqual(3, erfModel.Compute(1, true));
            Assert.AreEqual(6, erfModel.Compute(2, true));

            var random = new McraRandomGenerator();
            erfModel.ResampleExposureResponseFunction(random, false);
            Assert.AreNotEqual(3, erfModel.Compute(2, true));
            Assert.IsGreaterThan(2, erfModel.Compute(1, true));
            Assert.IsLessThan(4, erfModel.Compute(1, true));
            Assert.AreNotEqual(6, erfModel.Compute(2, true));
            Assert.IsGreaterThan(4, erfModel.Compute(2, true));
            Assert.IsLessThan(8, erfModel.Compute(2, true));
        }


        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeSubGroup_LeftInclusive() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = null,
                ERFUncertaintyDistribution = ExposureResponseSpecificationDistributionType.Normal,
                CounterFactualValue = 1.52,
                SubgroupIntervalBound = SubgroupIntervalBoundType.LeftInclusive,
                ErfSubgroups = [
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("2"),
                        ExposureUpper = 2.17,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("3"),
                        ExposureUpper = 2.95,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("4"),
                        ExposureUpper = double.PositiveInfinity,
                    }
                ]
            };

            var erfModel = ExposureResponseModelBuilder.Create(erf);
            Assert.AreEqual(1, erfModel.Compute(0, true));
            Assert.AreEqual(1, erfModel.Compute(1.51, true));
            Assert.AreEqual(2, erfModel.Compute(1.52, true));
            Assert.AreEqual(3, erfModel.Compute(2.17, true));
            Assert.AreEqual(4, erfModel.Compute(2.95, true));
            Assert.AreEqual(4, erfModel.Compute(3, true));
        }

        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeSubGroup_RightInclusive() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = null,
                ERFUncertaintyDistribution = ExposureResponseSpecificationDistributionType.Normal,
                CounterFactualValue = 1.52,
                SubgroupIntervalBound = SubgroupIntervalBoundType.RightInclusive,
                ErfSubgroups = [
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("2"),
                        ExposureUpper = 2.17,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("3"),
                        ExposureUpper = 2.95,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("4"),
                        ExposureUpper = double.PositiveInfinity,
                    }
                ]
            };

            var erfModel = ExposureResponseModelBuilder.Create(erf);
            Assert.AreEqual(1, erfModel.Compute(0, true));
            Assert.AreEqual(1, erfModel.Compute(1.51, true));
            Assert.AreEqual(2, erfModel.Compute(1.52, true));
            Assert.AreEqual(2, erfModel.Compute(2.17, true));
            Assert.AreEqual(3, erfModel.Compute(2.95, true));
            Assert.AreEqual(4, erfModel.Compute(3, true));
        }

        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeSubGroup_Uncertain() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = new NCalc.Expression("5.01"),
                ExposureResponseSpecificationLower = new NCalc.Expression("4.01"),
                ExposureResponseSpecificationUpper = new NCalc.Expression("6.01"),
                ERFUncertaintyDistribution = ExposureResponseSpecificationDistributionType.Normal,
                CounterFactualValue = 1,
                ErfSubgroups = [
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("6.01"),
                        ExposureUpper = 2,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("7.01"),
                    }
                ]
            };
            var erfModel = ExposureResponseModelBuilder.Create(erf);
            var random = new McraRandomGenerator();
            Assert.AreEqual(6.01, erfModel.Compute(2, true));
            Assert.AreEqual(7.01, erfModel.Compute(2.5, true));
            Assert.AreEqual(5.01, erfModel.Compute(2, false));
            Assert.AreEqual(5.01, erfModel.Compute(2.5, false));

            erfModel.ResampleExposureResponseFunction(random, false);
            Assert.IsGreaterThan(4, erfModel.Compute(2, false));
            Assert.IsGreaterThan(4, erfModel.Compute(2.5, false));
        }

        [TestMethod]
        public void ExposureResponseFunctionModel_TestComputeEqualDrawSubGroup() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ERFUncertaintyDistribution =  ExposureResponseSpecificationDistributionType.Normal,
                ExposureResponseSpecification = new NCalc.Expression("5"),
                ExposureResponseSpecificationUpper = new NCalc.Expression("6"),
                CounterFactualValue = 1,
                ErfSubgroups = [
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("5"),
                        ExposureResponseSpecificationUpper = new NCalc.Expression("6"),
                        ExposureUpper = 2,
                    },
                    new ErfSubgroup() {
                        ExposureResponseSpecification = new NCalc.Expression("5"),
                        ExposureResponseSpecificationUpper = new NCalc.Expression("6"),
                    }
                ]
            };
            var erfModel = ExposureResponseModelBuilder.Create(erf);
            var random = new McraRandomGenerator();
            Assert.AreEqual(5, erfModel.Compute(2, true));
            Assert.AreEqual(5, erfModel.Compute(2.5, true));

            erfModel.ResampleExposureResponseFunction(random, false);
            Assert.AreEqual(erfModel.Compute(2, true), erfModel.Compute(2.5, true));
        }
    }
}
