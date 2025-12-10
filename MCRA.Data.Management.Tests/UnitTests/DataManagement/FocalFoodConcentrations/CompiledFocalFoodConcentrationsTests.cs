using MCRA.Data.Management.Tests.UnitTests.DataManagement;
using MCRA.General;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    [TestClass]
    public class CompiledFocalFoodConcentrationsTests : CompiledTestsBase {
        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestFoodAndAnalysisSamples(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple")
            );

            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foods = GetAllFoods(managerType);
            var foodSamples = GetAllFocalFoodSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestFoodAndMissingAnalysisSamples(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesMissing")
            );

            var foodSamples = GetAllFocalFoodSamples(managerType);
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foods = GetAllFoods(managerType);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am2", "Am3" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS4" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

            Assert.AreEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, foodSamples.Values.SelectMany(c => c.SampleAnalyses).Sum(a => a.Concentrations.Count));
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestAnalyticalMethodCompounds(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );

            var foods = GetAllFoods(managerType);
            var compounds = GetAllCompounds(managerType);
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foodSamples = GetAllFocalFoodSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestAnalyticalMethodCompoundsFilterCompounds(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            var foods = GetAllFoods(managerType);
            var compounds = GetAllCompounds(managerType);
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foodSamples = GetAllFocalFoodSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

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

            Assert.IsEmpty(analyticalMethods["Am5"].AnalyticalMethodCompounds);
        }

        [TestMethod]
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestAnalyticalMethodCompoundsFilterFoodsCompounds(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple")
            );
            RawDataProvider.SetFilterCodes(ScopingType.Foods, ["A"]);
            RawDataProvider.SetFilterCodes(ScopingType.Compounds, ["P", "S"]);

            var foods = GetAllFoods(managerType);
            var compounds = GetAllCompounds(managerType);
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foodSamples = GetAllFocalFoodSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "A" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestAllDataSimple(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"FocalFoodsTests/FoodSamplesSimple"),
                (ScopingType.FocalFoodSampleAnalyses, @"FocalFoodsTests/AnalysisSamplesSimple"),
                (ScopingType.FocalFoodAnalyticalMethods, @"FocalFoodsTests/AnalyticalMethodsSimple"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"FocalFoodsTests/AnalyticalMethodCompoundsSimple"),
                (ScopingType.FocalFoodConcentrationsPerSample, @"FocalFoodsTests/ConcentrationsPerSampleSimple")
            );

            var foods = GetAllFoods(managerType);
            var compounds = GetAllCompounds(managerType);
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType);
            var foodSamples = GetAllFocalFoodSamples(managerType);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" },
                foodSamples.Values.SelectMany(c => c.SampleAnalyses).Select(c => c.Code).ToList());

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestDataGroups1(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest/FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest/SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest/AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest/AnalyticalMethodCompounds")
            );

            var foodSamples = GetAllFocalFoodSamples(managerType).Values;
            Assert.HasCount(20, foodSamples);

            var foods = GetAllFoods(managerType);
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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestDataGroups2(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest/FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest/SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest/AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest/AnalyticalMethodCompounds")
            );

            var foodSamples = GetAllFocalFoodSamples(managerType).Values;
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType).Values;

            Assert.HasCount(5, analyticalMethods);

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
        [DataRow(ManagerType.CompiledDataManager)]
        [DataRow(ManagerType.SubsetManager)]
        public void CompiledFocalFoods_TestDataGroups3(ManagerType managerType) {
            RawDataProvider.SetDataTables(
                (ScopingType.FocalFoodSamples, @"_DataGroupsTest/FoodSamples"),
                (ScopingType.FocalFoodSampleAnalyses, @"_DataGroupsTest/SampleAnalyses"),
                (ScopingType.FocalFoodAnalyticalMethods, @"_DataGroupsTest/AnalyticalMethods"),
                (ScopingType.FocalFoodAnalyticalMethodCompounds, @"_DataGroupsTest/AnalyticalMethodCompounds")
            );

            var foodSamples = GetAllFocalFoodSamples(managerType).Values;
            var analyticalMethods = GetAllFocalFoodAnalyticalMethods(managerType).Values;

            Assert.HasCount(5, analyticalMethods);

            var compounds = GetAllCompounds(managerType);
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
