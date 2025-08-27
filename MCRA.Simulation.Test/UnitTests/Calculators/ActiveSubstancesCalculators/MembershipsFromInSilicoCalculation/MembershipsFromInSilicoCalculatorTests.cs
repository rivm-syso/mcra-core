using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation {

    /// <summary>
    /// AssessmentGroupMemberships calculator
    /// </summary>
    [TestClass]
    public class MembershipsFromInSilicoCalculatorTests {

        /// <summary>
        /// Test calculation of available membership models with empty QSAR and docking collections.
        /// </summary>
        [TestMethod]
        public void MembershipsFromInSilicoCalculator_TestEmptyCollections() {
            var calculator = new MembershipsFromInSilicoCalculator(true, true);
            var effects = FakeEffectsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(3);
            var dockingModels = new List<MolecularDockingModel>();
            var qsarModels = new List<QsarMembershipModel>();
            var result = calculator.CalculateAvailableMembershipModels(dockingModels, qsarModels, substances, effects);
            Assert.AreEqual(result.Count, 0);
        }

        /// <summary>
        /// Test calculation of available membership models using a QSAR collections.
        /// </summary>
        [TestMethod]
        public void MembershipsFromInSilicoCalculator_TestComputeFromQsarMembershipModels() {
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(4);
            var qsarModels = new List<QsarMembershipModel>() {
                FakeQsarMembershipModelsGenerator.Create(focalEffect, substances, [0D, 0D, 1D, double.NaN]),
                FakeQsarMembershipModelsGenerator.Create(focalEffect, substances, [0D, 1D, 1D, double.NaN]),
                FakeQsarMembershipModelsGenerator.Create(focalEffect, substances, [1D, 1D, 1D, double.NaN]),
            };
            var calculator = new MembershipsFromInSilicoCalculator(false, true);
            var result = calculator.CalculateAvailableMembershipModels(null, qsarModels, substances, effects);
            CollectionAssert.AreEqual(result[0].MembershipProbabilities.Values.ToArray(), new[] { 0D, 0D, 1D });
            CollectionAssert.AreEqual(result[1].MembershipProbabilities.Values.ToArray(), new[] { 0D, 1D, 1D });
            CollectionAssert.AreEqual(result[2].MembershipProbabilities.Values.ToArray(), new[] { 1D, 1D, 1D });
        }

        /// <summary>
        /// Test calculation of available membership models using a docking models collections.
        /// </summary>
        [TestMethod]
        public void MembershipsFromInSilicoCalculator_TestComputeFromDockingModels() {
            var effects = FakeEffectsGenerator.Create(1);
            var focalEffect = effects.First();
            var substances = FakeSubstancesGenerator.Create(4);
            double threshold = -6;
            var dockingModels = new List<MolecularDockingModel>() {
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -5, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-5, -7, -7, double.NaN]),
                FakeMolecularDockingModelsGenerator.Create(focalEffect, substances, threshold, [-7, -7, -7, double.NaN]),
            };
            var calculator = new MembershipsFromInSilicoCalculator(true, false);
            var result = calculator.CalculateAvailableMembershipModels(dockingModels, null, substances, effects);
            CollectionAssert.AreEqual(result[0].MembershipProbabilities.Values.ToArray(), new[] { 0D, 0D, 1D });
            CollectionAssert.AreEqual(result[1].MembershipProbabilities.Values.ToArray(), new[] { 0D, 1D, 1D });
            CollectionAssert.AreEqual(result[2].MembershipProbabilities.Values.ToArray(), new[] { 1D, 1D, 1D });
        }
    }
}
