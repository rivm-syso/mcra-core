using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledProcessingTests : CompiledTestsBase {

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledProcessing_TestSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests/ProcessingFactorsSimple")
            );

            var factors = GetAllProcessingFactors(managerType);

            Assert.AreEqual(3, factors.Count(f => f.Compound == null));
            Assert.AreEqual(8, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var processedCodes = factors.Where(f => f.FoodProcessed != null).Select(r => r.FoodProcessed.Code).Distinct();
            var unprocessedCodes = factors.Select(f => f.FoodUnprocessed.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D", "E" }, compoundCodes.ToList());
            Assert.IsTrue(!processedCodes?.Any() ?? true);
            CollectionAssert.AreEquivalent(new[] { "f2", "f3", "f4", "f5", "f6" }, unprocessedCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledProcessing_TestSimpleFoodsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests/ProcessingFactorsSimpleOld")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f2"]);

            var factors = GetAllProcessingFactors(managerType);
            Assert.AreEqual(1, factors.Count(f => f.Compound == null));
            Assert.AreEqual(3, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var processedCodes = factors.Select(f => f.FoodProcessed.Code).Distinct();
            var unprocessedCodes = factors.Select(f => f.FoodUnprocessed.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "A", "D", "E" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1" }, processedCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f2" }, unprocessedCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledProcessing_TestSimpleCompoundsFilter(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests/ProcessingFactorsSimpleOld")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllProcessingFactors(managerType);

            Assert.AreEqual(3, factors.Count(f => f.Compound == null));
            Assert.AreEqual(4, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var processedCodes = factors.Select(f => f.FoodProcessed.Code).Distinct();
            var unprocessedCodes = factors.Select(f => f.FoodUnprocessed.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "B", "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2", "f3" }, processedCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f2", "f3", "f4", "f5" }, unprocessedCodes.ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledProcessingFactors_TestFilterFoodsAndCompoundsSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.ProcessingFactors, @"ProcessingTests/ProcessingFactorsSimpleOld")
            );
            //set a filter scope on Foods
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["f1", "f2", "f4"]);
            //set a filter scope on compounds
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["B", "C"]);

            var factors = GetAllProcessingFactors(managerType);

            Assert.AreEqual(1, factors.Count(f => f.Compound == null));
            Assert.AreEqual(2, factors.Count(f => f.Compound != null));
            var compoundCodes = factors.Where(f => f.Compound != null).Select(f => f.Compound.Code).Distinct();
            var processedCodes = factors.Select(f => f.FoodProcessed.Code).Distinct();
            var unprocessedCodes = factors.Select(f => f.FoodUnprocessed.Code).Distinct();

            CollectionAssert.AreEquivalent(new[] { "C" }, compoundCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f1", "f2" }, processedCodes.ToList());
            CollectionAssert.AreEquivalent(new[] { "f2", "f4" }, unprocessedCodes.ToList());
        }
    }
}
