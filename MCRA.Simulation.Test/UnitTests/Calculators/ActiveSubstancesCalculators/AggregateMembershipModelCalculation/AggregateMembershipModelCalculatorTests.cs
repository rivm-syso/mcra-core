using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation {

    /// <summary>
    /// Aggregate membership model calculator tests.
    /// </summary>
    [TestClass]
    public class AggregateMembershipModelCalculatorTests {

        /// <summary>
        /// Test empty lists and ActiveSubstanceCalculationMethod.CrispMajority
        /// </summary>
        [TestMethod]
        public void AggregateMembershipModelCalculator_TestComputeEmpty() {
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(5);
            var dockingModels = new List<MolecularDockingModel>();
            var models = new List<ActiveSubstanceModel>();
            var settings = new MockAggregateMembershipModelCalculatorSettings() {
                AssessmentGroupMembershipCalculationMethod = AssessmentGroupMembershipCalculationMethod.CrispMajority,
                IncludeSubstancesWithUnknowMemberships = true,
                PriorMembershipProbability = 0,
                UseProbabilisticMemberships = false,
            };
            var calculator = new AggregateMembershipModelCalculator(settings);
            var result = calculator.Compute(models, substances, focalEffect, null);
            Assert.AreEqual(result.MembershipProbabilities.Count, substances.Count);
            Assert.IsTrue(result.MembershipProbabilities.All(r => r.Value == 1D));
        }

        /// <summary>
        /// Test ActiveSubstanceCalculationMethod.CrispMajority
        /// </summary>
        [TestMethod]
        public void AggregateMembershipModelCalculator_TestMajority() {
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(5);
            var models = mockAssessmentGroupMembershipModels(focalEffect, substances);
            var settings = new MockAggregateMembershipModelCalculatorSettings() {
                AssessmentGroupMembershipCalculationMethod = AssessmentGroupMembershipCalculationMethod.CrispMajority,
                IncludeSubstancesWithUnknowMemberships = true,
                PriorMembershipProbability = 0,
                UseProbabilisticMemberships = false,
            };
            var calculator = new AggregateMembershipModelCalculator(settings);
            var result = calculator.Compute(models, substances, focalEffect, null);
            Assert.AreEqual(result.MembershipProbabilities.Count, substances.Count);
            CollectionAssert.AreEquivalent(
                new[] { 0D, 0D, 1D, 1D, 1D },
                result.MembershipProbabilities.Values.ToArray()
            );
        }

        /// <summary>
        /// Test ActiveSubstanceCalculationMethod.CrispMax
        /// </summary>
        [TestMethod]
        public void AggregateMembershipModelCalculator_TestMax() {
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(5);
            var models = mockAssessmentGroupMembershipModels(focalEffect, substances);
            var settings = new MockAggregateMembershipModelCalculatorSettings() {
                AssessmentGroupMembershipCalculationMethod = AssessmentGroupMembershipCalculationMethod.CrispMax,
                IncludeSubstancesWithUnknowMemberships = true,
                PriorMembershipProbability = 0,
                UseProbabilisticMemberships = false,
            };
            var calculator = new AggregateMembershipModelCalculator(settings);
            var result = calculator.Compute(models, substances, focalEffect, null);
            Assert.AreEqual(result.MembershipProbabilities.Count, substances.Count);
            CollectionAssert.AreEquivalent(
                new[] { 0D, 1D, 1D, 1D, 1D },
                result.MembershipProbabilities.Values.ToArray()
            );
        }

        /// <summary>
        /// Test ActiveSubstanceCalculationMethod.ProbabilisticRatio
        /// </summary>
        [TestMethod]
        public void AggregateMembershipModelCalculator_TestRatio() {
            var effects = MockEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = MockSubstancesGenerator.Create(5);
            var models = mockAssessmentGroupMembershipModels(focalEffect, substances);
            var settings = new MockAggregateMembershipModelCalculatorSettings() {
                AssessmentGroupMembershipCalculationMethod = AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio,
                IncludeSubstancesWithUnknowMemberships = true,
                PriorMembershipProbability = 0,
                UseProbabilisticMemberships = false,
            };
            var calculator = new AggregateMembershipModelCalculator(settings);
            var result = calculator.Compute(models, substances, focalEffect, null);
            Assert.AreEqual(result.MembershipProbabilities.Count, substances.Count);
            CollectionAssert.AreEquivalent(
                new[] { 0D, 0.25, 0.5, 0.75, 1D },
                result.MembershipProbabilities.Values.ToArray()
            );
        }

        private static List<ActiveSubstanceModel> mockAssessmentGroupMembershipModels(Effect focalEffect, List<Compound> substances) {
            return new List<ActiveSubstanceModel>() {
                MockAssessmentGroupMembershipModelsGenerator.Create(focalEffect, substances, new [] { 0D, 0D, 0D, 0D, 1D }),
                MockAssessmentGroupMembershipModelsGenerator.Create(focalEffect, substances, new [] { 0D, 0D, 0D, 1D, 1D }),
                MockAssessmentGroupMembershipModelsGenerator.Create(focalEffect, substances, new [] { 0D, 0D, 1D, 1D, 1D }),
                MockAssessmentGroupMembershipModelsGenerator.Create(focalEffect, substances, new [] { 0D, 1D, 1D, 1D, 1D }),
            };
        }
    }
}
