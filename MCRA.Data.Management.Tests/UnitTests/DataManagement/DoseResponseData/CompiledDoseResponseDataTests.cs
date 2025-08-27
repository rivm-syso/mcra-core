using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledDoseResponseDataTests : CompiledTestsBase {
        protected Func<IDictionary<string, DoseResponseExperiment>> _getExperimentsDelegate;
        protected Func<IDictionary<string, Response>> _getResponsesDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledDoseResponseExperiments_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsSimple"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            var experiments = _getExperimentsDelegate.Invoke();
            //only experiments with all matching codes are loaded
            //(matching response codes are mandatory)
            Assert.AreEqual(2, experiments.Count);

            var substances = _getSubstancesDelegate.Invoke();

            //Substances are loaded from valid experiments
            Assert.AreEqual(5, substances.Count);

            var responses = _getResponsesDelegate.Invoke();

            Assert.AreEqual(3, responses.Count);
        }

        [TestMethod]
        public void CompiledDoseResponseExperiments_TestFilterSubstancesResponses() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var experiments = _getExperimentsDelegate.Invoke();

            Assert.AreEqual(6, experiments.Count);
            CollectionAssert.AreEquivalent(
                new[] { "x10", "x11", "x14", "x19", "x20", "x23" },
                experiments.Keys.ToList()
            );
        }

        [TestMethod]
        public void CompiledDoseResponseExperiments_TestFilterResponses() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Responses, ["R2"]);

            var experiments = _getExperimentsDelegate.Invoke();

            Assert.AreEqual(18, experiments.Count);
            CollectionAssert.AreEquivalent(
                new[] { "x10", "x11", "x12", "x13", "x14", "x15", "x16", "x17", "x18", "x19", "x20", "x21", "x22", "x23", "x24", "x25", "x26", "x27" },
                experiments.Keys.ToList()
            );
        }

        [TestMethod]
        public void CompiledDoseResponseExperiments_TestFilterSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.DoseResponseExperiments, @"DoseResponseTests/ExperimentsForFiltering"),
                (ScopingType.Responses, @"DoseResponseTests/ResponsesSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["A", "B"]);

            var experiments = _getExperimentsDelegate.Invoke();

            Assert.AreEqual(12, experiments.Count);
            CollectionAssert.AreEquivalent(
                new[] { "x01", "x02", "x05", "x10", "x11", "x14", "x19", "x20", "x23", "x28", "x29", "x32" },
                experiments.Keys.ToList()
            );
        }
    }
}
