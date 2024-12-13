using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledFocalFoodConcentrationsTests : CompiledTestsBase {

        protected Func<IDictionary<string, AnalyticalMethod>> _getFocalFoodsAnalyticalMethodsDelegate;
        protected Func<IDictionary<string, FoodSample>> _getAllFocalFoodSamplesDelegate;
        protected Func<IDictionary<string, Food>> _getAllFocalCommodityFoodsDelegate;
        protected Func<IDictionary<string, Food>> _getFoodsDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;
        protected bool _isSubSetManagerTest = false;

        [TestInitialize]
        public override void TestInitialize() {
            base.TestInitialize();
            //explicitly set data sources
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
        }

        [TestMethod]
        public void CompiledFocalFoods_TestFoodAndAnalysisSamples() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSimple")
            );

            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();
            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());
        }

        [TestMethod]
        public void CompiledFocalFoods_TestFoodAndMissingAnalysisSamples() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesMissing")
            );

            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am2", "Am3" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS4" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));
        }

        [TestMethod]
        public void CompiledFocalFoods_TestAnalyticalMethodCompounds() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple")
            );

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));

            var compoundCodes = analyticalMethods["Am1"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am2"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q" }, compoundCodes);

            compoundCodes = analyticalMethods["Am3"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, compoundCodes);

            compoundCodes = analyticalMethods["Am4"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "T" }, compoundCodes);

            compoundCodes = analyticalMethods["Am5"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Z" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledFocalFoods_TestAnalyticalMethodCompoundsFilterCompounds() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));

            var compoundCodes = analyticalMethods["Am1"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am2"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P" }, compoundCodes);

            compoundCodes = analyticalMethods["Am3"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am4"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P" }, compoundCodes);

            Assert.AreEqual(0, analyticalMethods["Am5"].AnalyticalMethodCompounds.Count);
        }

        [TestMethod]
        public void CompiledFocalFoods_TestAnalyticalMethodCompoundsFilterFoodsCompounds() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple")
            );
            _rawDataProvider.SetFilterCodes(ScopingType.Foods, ["A"]);
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));

            var compoundCodes = analyticalMethods["Am1"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am2"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P" }, compoundCodes);

            compoundCodes = analyticalMethods["Am3"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am4"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P" }, compoundCodes);
        }

        [TestMethod]
        public void CompiledFocalFoods_TestAllDataSimple() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests\FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests\AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests\AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.FocalFoodConcentrationsPerSample, @"FocalFoodsTests\ConcentrationsPerSampleSimple")
            );

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreNotEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));

            var compoundCodes = analyticalMethods["Am1"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S" }, compoundCodes);

            compoundCodes = analyticalMethods["Am2"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q" }, compoundCodes);

            compoundCodes = analyticalMethods["Am3"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, compoundCodes);

            compoundCodes = analyticalMethods["Am4"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "T" }, compoundCodes);

            compoundCodes = analyticalMethods["Am5"].AnalyticalMethodCompounds.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Z" }, compoundCodes);

            compoundCodes = foodSamples["FS1"].SampleAnalyses[0].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q" }, compoundCodes);

            compoundCodes = foodSamples["FS2"].SampleAnalyses[0].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "R" }, compoundCodes);

            compoundCodes = foodSamples["FS3"].SampleAnalyses[0].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q" }, compoundCodes);

            compoundCodes = foodSamples["FS4"].SampleAnalyses[0].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, compoundCodes);

            compoundCodes = foodSamples["FS4"].SampleAnalyses[1].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Z" }, compoundCodes);
        }


        /// <summary>
        /// Tests correct loading of the samples. Verification by counting the samples for the
        /// foods apple (=5), bananas (=5), and pineapple (=10) of the compiled datasource generated
        /// with the specified concentration data.
        /// </summary>
        [TestMethod]
        public void CompiledFocalFoods_TestDataGroups1() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke().Values;
            Assert.AreEqual(20, foodSamples.Count);

            var foods = _getFoodsDelegate.Invoke();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            Assert.AreEqual(5, foodSamples.Count(s => s.Food == foodApple));
            Assert.AreEqual(5, foodSamples.Count(s => s.Food == foodBananas));
            Assert.AreEqual(10, foodSamples.Count(s => s.Food == foodPineapple));
        }

        /// <summary>
        /// Test correct loading of the analytical methods and the substance specific details of the
        /// analytical methods (analytical methods - compounds). Verification by counting the
        /// analytical methods (=5), and the number of samples per analytical method (AM1 = 5, AM2
        ///  = 5, AM3 = 1, AM4 = 3, and AM5 = 6).
        /// </summary>
        [TestMethod]
        public void CompiledFocalFoods_TestDataGroups2() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke().Values;
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke().Values;

            Assert.AreEqual(5, analyticalMethods.Count);

            var analyticalMethod1 = analyticalMethods.Single(am => am.Code == "AM1");
            var analyticalMethod2 = analyticalMethods.Single(am => am.Code == "AM2");
            var analyticalMethod3 = analyticalMethods.Single(am => am.Code == "AM3");
            var analyticalMethod4 = analyticalMethods.Single(am => am.Code == "AM4");
            var analyticalMethod5 = analyticalMethods.Single(am => am.Code == "AM5");

            var samplesAnalyticalMethod1 = foodSamples.SelectMany(c => c.SampleAnalyses).Where(s => s.AnalyticalMethod == analyticalMethod1);
            var samplesAnalyticalMethod2 = foodSamples.SelectMany(c => c.SampleAnalyses).Where(s => s.AnalyticalMethod == analyticalMethod2);
            var samplesAnalyticalMethod3 = foodSamples.SelectMany(c => c.SampleAnalyses).Where(s => s.AnalyticalMethod == analyticalMethod3);
            var samplesAnalyticalMethod4 = foodSamples.SelectMany(c => c.SampleAnalyses).Where(s => s.AnalyticalMethod == analyticalMethod4);
            var samplesAnalyticalMethod5 = foodSamples.SelectMany(c => c.SampleAnalyses).Where(s => s.AnalyticalMethod == analyticalMethod5);

            Assert.AreEqual(5, samplesAnalyticalMethod1.Count());
            Assert.AreEqual(5, samplesAnalyticalMethod2.Count());
            Assert.AreEqual(1, samplesAnalyticalMethod3.Count());
            Assert.AreEqual(3, samplesAnalyticalMethod4.Count());
            Assert.AreEqual(6, samplesAnalyticalMethod5.Count());
        }

        /// <summary>
        /// Test correct loading of the analytical methods and the substance specific details of the
        /// analytical methods (analytical methods - compounds). The analytical methods per
        /// substance (A = 5, B = 2, C = 2, D = 3).
        /// </summary>
        [TestMethod]
        public void CompiledFocalFoods_TestDataGroups3() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getAllFocalFoodSamplesDelegate.Invoke().Values;
            var analyticalMethods = _getFocalFoodsAnalyticalMethodsDelegate.Invoke().Values;

            Assert.AreEqual(5, analyticalMethods.Count);

            var compounds = _getSubstancesDelegate.Invoke();
            var compoundA = compounds["CompoundA"];
            var compoundB = compounds["CompoundB"];
            var compoundC = compounds["CompoundC"];
            var compoundD = compounds["CompoundD"];

            var allAnalyticalMethodCompounds = analyticalMethods.SelectMany(a => a.AnalyticalMethodCompounds);
            Assert.AreEqual(5, allAnalyticalMethodCompounds.Count(a => a.Key == compoundA));
            Assert.AreEqual(2, allAnalyticalMethodCompounds.Count(a => a.Key == compoundB));
            Assert.AreEqual(2, allAnalyticalMethodCompounds.Count(a => a.Key == compoundC));
            Assert.AreEqual(3, allAnalyticalMethodCompounds.Count(a => a.Key == compoundD));
        }
    }
}
