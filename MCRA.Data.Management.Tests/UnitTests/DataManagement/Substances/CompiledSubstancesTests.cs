using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledSubstancesTests : CompiledTestsBase {
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledSubstances_TestGetAllSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple")
            );

            var substances = _getSubstancesDelegate.Invoke();

            Assert.HasCount(5, substances);
            Assert.IsTrue(substances.TryGetValue("A", out var c) && c.Name.Equals("SubstanceA"));
            Assert.IsTrue(substances.TryGetValue("B", out c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("C", out c) && c.Name.Equals("SubstanceC"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
            Assert.IsTrue(substances.TryGetValue("E", out c) && c.Name.Equals("SubstanceE"));
        }

        [TestMethod]
        public void CompiledSubstances_TestGetAllSubstancesFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests/SubstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "D"]);

            var substances = _getSubstancesDelegate.Invoke();

            Assert.HasCount(2, substances);
            Assert.IsTrue(substances.TryGetValue("B", out var c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
        }
    }
}
