using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledInterSpeciesFactorsTests : CompiledTestsBase {

        protected Func<ICollection<InterSpeciesFactor>> _getItemsDelegate;

        [TestMethod]
        public void CompiledInterSpeciesFactorsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(2, factors.Count(f => f.Compound == null));
            Assert.AreEqual(9, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5", "Eff6" }, effectCodes.ToList());
        }


        [TestMethod]
        public void CompiledInterSpeciesFactorsSimpleEffectsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1" });

            var factors = _getItemsDelegate.Invoke();
            Assert.AreEqual(1, factors.Count);
            var f = factors.First();

            Assert.IsNull(f.Compound);
            Assert.AreEqual("Eff1", f.Effect.Code);
        }

        [TestMethod]
        public void CompiledInterSpeciesFactorsSimpleCompoundsFilterTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(2, factors.Count(f => f.Compound == null));
            Assert.AreEqual(4, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff2", "Eff3", "Eff4", "Eff5" }, effectCodes.ToList());
        }

        [TestMethod]
        public void CompiledInterSpeciesModelParametersFilterEffectsAndCompoundsSimpleTest() {
            _rawDataProvider.SetDataTables(
                (ScopingType.InterSpeciesModelParameters, @"InterSpeciesFactorsTests\InterSpeciesModelParametersSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Effects, new[] { "Eff1", "Eff4" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var factors = _getItemsDelegate.Invoke();

            Assert.AreEqual(1, factors.Count(f => f.Compound == null));
            Assert.AreEqual(1, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var effectCodes = factors.Select(f => f.Effect.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "Eff1", "Eff4" }, effectCodes.ToList());
        }
    }
}
