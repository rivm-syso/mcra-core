using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledEffectRepresentationsTests : CompiledTestsBase {
        protected Func<IList<EffectRepresentation>> _getItemsDelegate;

        [TestMethod]
        public void CompiledEffectRepresentations_TestOnly() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple")
            );

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(0, representations.Count);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_TestOnlyWithResponsesScope() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Responses);
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple")
            );

            //set a filter scope on responses
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(0, representations.Count);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests\ResponsesSimple")
            );
            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(9, representations.Count);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1", "R2", "R3" }, responses);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_TestFilterEffectsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff1"]);

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(1, representations.Count);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1" }, responses);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_FilterResponsesSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests\ResponsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(3, representations.Count);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff2", "Eff3", "Eff4" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R2" }, responses);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_TestFilterTestsystemsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple"),
                (ScopingType.Responses, @"EffectRepresentationsTests\ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests\TestSystemsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(8, representations.Count);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R1", "R2" }, responses);
        }

        [TestMethod]
        public void CompiledEffectRepresentations_TestFilterAll() {
            _rawDataProvider.SetDataTables(
                (ScopingType.EffectRepresentations, @"EffectRepresentationsTests\EffectRepresentationsSimple"),
                (ScopingType.Responses, @"DoseResponseTests\ResponsesSimple"),
                (ScopingType.TestSystems, @"DoseResponseTests\TestSystemsSimple"),
                (ScopingType.Effects, @"EffectsTests\EffectsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.TestSystems, ["sys1"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, ["Eff2", "Eff5"]);

            var representations = _getItemsDelegate.Invoke();

            Assert.AreEqual(1, representations.Count);

            var effects = representations.Select(r => r.Effect.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "Eff2" }, effects);

            var responses = representations.Select(r => r.Response.Code).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { "R2" }, responses);
        }
    }
}
