using MCRA.Data.Compiled.Objects;
using MCRA.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.Test.UnitTests.DataManagement {
    public class CompiledConcentrationsTests : CompiledTestsBase {
        protected Func<IDictionary<string, AnalyticalMethod>> _getAnalyticalMethodsDelegate;
        protected Func<IDictionary<string, FoodSample>> _getFoodSamplesDelegate;
        protected Func<IDictionary<string, Food>> _getFoodsDelegate;
        protected Func<IDictionary<string, Compound>> _getSubstancesDelegate;
        protected bool _isSubSetManagerTest = false;

        [TestMethod]
        public void CompiledConcentrations_TestFoodAndAnalysisSamples() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple")
            );

            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();
            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);
            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2", "FS3", "FS4" }, foodSamples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, sampleAnalyses.Keys.ToList());
        }

        [TestMethod]
        public void CompiledConcentrations_TestFoodAndMissingAnalysisSamples() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesMissing")
            );

            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foods = _getFoodsDelegate.Invoke();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am2", "Am3" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2", "FS3", "FS4" }, foodSamples.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS4"}, sampleAnalyses.Keys.ToList());

            Assert.AreEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, sampleAnalyses.Values.Sum(a => a.Concentrations.Count));
        }

        [TestMethod]
        public void CompiledConcentrations_TestAnalyticalMethodCompounds() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple")
            );

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, sampleAnalyses.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2", "FS3", "FS4" }, foodSamples.Keys.ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, sampleAnalyses.Values.Sum(a => a.Concentrations.Count));

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
        public void CompiledConcentrations_TestAnalyticalMethodCompoundsFilterCompounds() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "P", "S" });

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, sampleAnalyses.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2", "FS3", "FS4" }, foodSamples.Keys.ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, sampleAnalyses.Values.Sum(a => a.Concentrations.Count));

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
        public void CompiledConcentrations_TestAnalyticalMethodCompoundsFilterFoodsCompounds() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple")
            );

            _rawDataProvider.SetFilterCodes(ScopingType.Foods, new[] { "A" });
            _rawDataProvider.SetFilterCodes(ScopingType.Compounds, new[] { "P", "S" });

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);

            CollectionAssert.AreEquivalent(new[] { "A" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "S" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2" }, sampleAnalyses.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2" }, foodSamples.Keys.ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreEqual(0, sampleAnalyses.Values.Sum(a => a.Concentrations.Count));

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
        public void CompiledConcentrations_TestAllDataSimple() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple"),
                (ScopingType.ConcentrationsPerSample, @"ConcentrationsTests\ConcentrationsPerSampleSimple")
            );

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();
            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);

            CollectionAssert.AreEquivalent(new[] { "A", "B", "D" }, foods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "P", "Q", "R", "S", "T", "Z" }, compounds.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "Am1", "Am2", "Am3", "Am4", "Am5" }, analyticalMethods.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "AS1", "AS2", "AS3", "AS4", "AS5" }, sampleAnalyses.Keys.ToList());
            CollectionAssert.AreEquivalent(new[] { "FS1", "FS2", "FS3", "FS4" }, foodSamples.Keys.ToList());

            Assert.AreNotEqual(0, analyticalMethods.Values.Sum(a => a.AnalyticalMethodCompounds.Count));
            Assert.AreNotEqual(0, sampleAnalyses.Values.Sum(a => a.Concentrations.Count));

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

            compoundCodes = sampleAnalyses["AS1"].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "Q" }, compoundCodes);

            compoundCodes = sampleAnalyses["AS2"].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "P", "R" }, compoundCodes);

            compoundCodes = sampleAnalyses["AS3"].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q" }, compoundCodes);

            compoundCodes = sampleAnalyses["AS4"].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Q", "S", "T" }, compoundCodes);

            compoundCodes = sampleAnalyses["AS5"].Concentrations.Keys.Select(c => c.Code).ToList();
            CollectionAssert.AreEquivalent(new[] { "Z" }, compoundCodes);
        }

        [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
        public void CompiledConcentrations_TestMissingMethodCompounds() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSimple"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSimple"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsMissing"),
                (ScopingType.ConcentrationsPerSample, @"ConcentrationsTests\ConcentrationsPerSampleSimple")
            );

            var foods = _getFoodsDelegate.Invoke();
            var compounds = _getSubstancesDelegate.Invoke();
            //an exception will be thrown when the samples are loaded as part of getting the analyticalmethods
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke();

        }


        /// <summary>
        /// Test correct loading of additional sample properties.
        /// </summary>
        [TestMethod]
        public void CompiledConcentrations_TestAdditionalSampleProperties() {
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"ConcentrationsTests\FoodSamplesSubset"),
                (ScopingType.FoodSamplePropertyValues, @"ConcentrationsTests\FoodSamplesSubsetPropertyValues"),
                (ScopingType.SampleAnalyses, @"ConcentrationsTests\AnalysisSamplesSubset"),
                (ScopingType.AnalyticalMethods, @"ConcentrationsTests\AnalyticalMethodsSimple"),
                (ScopingType.AnalyticalMethodCompounds, @"ConcentrationsTests\AnalyticalMethodCompoundsSimple")
            );

            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var properties = foodSamples
                .SelectMany(r => r.Value.SampleProperties)
                .GroupBy(r => r.Key)
                .ToList();
        }

        /// <summary>
        /// Tests correct loading of the samples. Verification by counting the samples for the
        /// foods apple (=5), bananas (=5), and pineapple (=10) of the compiled datasource generated
        /// with the specified concentration data.
        /// </summary>
        [TestMethod]
        public void CompiledConcentrations_TestDataGroups1() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.SampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.AnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.AnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);
            Assert.AreEqual(20, sampleAnalyses.Count);

            var foods = _getFoodsDelegate.Invoke();
            var foodApple = foods["APPLE"];
            var foodBananas = foods["BANANAS"];
            var foodPineapple = foods["PINEAPPLE"];

            Assert.AreEqual(5, foodSamples.Count(s => s.Value.Food == foodApple));
            Assert.AreEqual(5, foodSamples.Count(s => s.Value.Food == foodBananas));
            Assert.AreEqual(10, foodSamples.Count(s => s.Value.Food == foodPineapple));
        }

        /// <summary>
        /// Test correct loading of the analytical methods and the substance specific details of the
        /// analytical methods (analytical methods - compounds). Verification by counting the
        /// analytical methods (=5), and the number of samples per analytical method (AM1 = 5, AM2
        ///  = 5, AM3 = 1, AM4 = 3, and AM5 = 6).
        /// </summary>
        [TestMethod]
        public void CompiledConcentrations_TestDataGroups2() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.SampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.AnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.AnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke().Values;

            Assert.AreEqual(5, analyticalMethods.Count);

            var analyticalMethod1 = analyticalMethods.Single(am => am.Code == "AM1");
            var analyticalMethod2 = analyticalMethods.Single(am => am.Code == "AM2");
            var analyticalMethod3 = analyticalMethods.Single(am => am.Code == "AM3");
            var analyticalMethod4 = analyticalMethods.Single(am => am.Code == "AM4");
            var analyticalMethod5 = analyticalMethods.Single(am => am.Code == "AM5");

            var samplesAnalyticalMethod1 = sampleAnalyses.Where(s => s.Value.AnalyticalMethod == analyticalMethod1);
            var samplesAnalyticalMethod2 = sampleAnalyses.Where(s => s.Value.AnalyticalMethod == analyticalMethod2);
            var samplesAnalyticalMethod3 = sampleAnalyses.Where(s => s.Value.AnalyticalMethod == analyticalMethod3);
            var samplesAnalyticalMethod4 = sampleAnalyses.Where(s => s.Value.AnalyticalMethod == analyticalMethod4);
            var samplesAnalyticalMethod5 = sampleAnalyses.Where(s => s.Value.AnalyticalMethod == analyticalMethod5);

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
        public void CompiledConcentrations_TestDataGroups3() {
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Foods);
            _rawDataProvider.SetEmptyDataSource(SourceTableGroup.Compounds);
            _rawDataProvider.SetDataTables(
                (ScopingType.FoodSamples, @"_DataGroupsTest\FoodSamples"),
                (ScopingType.SampleAnalyses, @"_DataGroupsTest\SampleAnalyses"),
                (ScopingType.AnalyticalMethods, @"_DataGroupsTest\AnalyticalMethods"),
                (ScopingType.AnalyticalMethodCompounds, @"_DataGroupsTest\AnalyticalMethodCompounds")
            );

            var foodSamples = _getFoodSamplesDelegate.Invoke();
            var sampleAnalyses = foodSamples.SelectMany(c => c.Value.SampleAnalyses).ToDictionary(c => c.Code);
            var analyticalMethods = _getAnalyticalMethodsDelegate.Invoke().Values;

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
