using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledSubstancesTests : CompiledTestsBase {
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;

        [TestMethod]
        public void CompiledSubstances_TestGetAllSubstances() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests\SubstancesSimple")
            );

            var substances = _getSubstancesDelegate.Invoke();

            Assert.AreEqual(5, substances.Count);
            Compound c;
            Assert.IsTrue(substances.TryGetValue("A", out c) && c.Name.Equals("SubstanceA"));
            Assert.IsTrue(substances.TryGetValue("B", out c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("C", out c) && c.Name.Equals("SubstanceC"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
            Assert.IsTrue(substances.TryGetValue("E", out c) && c.Name.Equals("SubstanceE"));
        }

        [TestMethod]
        public void CompiledSubstances_TestGetAllSubstancesFiltered() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests\SubstancesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "D" });

            var substances = _getSubstancesDelegate.Invoke();

            Assert.AreEqual(2, substances.Count);
            Compound c;
            Assert.IsTrue(substances.TryGetValue("B", out c) && c.Name.Equals("SubstanceB"));
            Assert.IsTrue(substances.TryGetValue("D", out c) && c.Name.Equals("SubstanceD"));
        }
    }
}
