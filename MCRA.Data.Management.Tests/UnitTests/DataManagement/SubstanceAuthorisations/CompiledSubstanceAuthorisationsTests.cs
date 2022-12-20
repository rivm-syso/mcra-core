using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledSubstanceAuthorisationsTests : CompiledTestsBase {
        protected Func<ICollection<SubstanceAuthorisation>> _getItemsDelegate;

        [TestMethod]
        public void CompiledSubstanceAuthorisations_TestSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests\AuthorisedUsesSimple")
            );

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E", "F" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5", "f6" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledSubstanceAuthorisations_TestSimpleFoodsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests\AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1" });

            var records = _getItemsDelegate.Invoke();
            Assert.AreEqual(1, records.Count);
            var f = records.First();

            Assert.AreEqual("B", f.Substance.Code);
            Assert.AreEqual("f1", f.Food.Code);
        }

        [TestMethod]
        public void CompiledSubstanceAuthorisations_TestSimpleCompoundsFilter() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests\AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3", "f4", "f5" }, foodCodes.ToList());
        }

        [TestMethod]
        public void CompiledSubstanceAuthorisations_TestFilterFoodsAndCompoundsSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.SubstanceAuthorisations, @"AuthorisedUsesTests\AuthorisedUsesSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "f1", "f4" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "B", "C" });

            var records = _getItemsDelegate.Invoke();

            var compoundCodes = records.Select(f => f.Substance.Code).Distinct();
            var foodCodes = records.Select(f => f.Food.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f4" }, foodCodes.ToList());
        }
    }
}
