using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class SubsetManagerSubstancesTests : SubsetManagerTestsBase {
        [TestMethod]
        public void SubsetManager_TestReferenceCompound() {
            _rawDataProvider.SetDataTables(
                (ScopingType.Compounds, @"SubstancesTests\SubstancesSimple")
            );

            var refCompound = _subsetManager.ReferenceCompound;
            Assert.IsNull(refCompound);

            _project.EffectSettings.CodeReferenceCompound = "ZZ";
            refCompound = _subsetManager.ReferenceCompound;
            Assert.IsNull(refCompound);

            _project.EffectSettings.CodeReferenceCompound = "c";

            refCompound = _subsetManager.ReferenceCompound;
            Assert.AreEqual("C", refCompound.Code);
            Assert.AreEqual(_subsetManager.AllCompoundsByCode["c"], refCompound);
        }
    }
}
