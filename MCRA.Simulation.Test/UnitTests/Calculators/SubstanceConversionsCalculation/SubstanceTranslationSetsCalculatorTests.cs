using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SubstanceTranslationsCalculation {
    /// <summary>
    /// SubstanceTranslationsCalculation calculator
    /// </summary>
    [TestClass]
    public class SubstanceTranslationSetsCalculatorTests {

        /// <summary>
        /// Calculate translations sets: general translation sets
        /// </summary>
        [TestMethod]
        public void SubstanceTranslationSetsCalculatorTests_TestComputeGeneralTranslationSets() {
            var dimethoateRd = new Compound("CompoundARD");
            var dimethoate = new Compound("CompoundA");
            var omethoate = new Compound("CompoundE");
            var substanceConversions = createMetaboliteSubstanceConversions(dimethoateRd, dimethoate, omethoate);

            var calculator = new SubstanceConversionSetsCalculator();
            var generalTranslationSets = calculator.ComputeGeneralTranslationSets(substanceConversions.ToLookup(r => r.MeasuredSubstance));
            Assert.AreEqual(1, generalTranslationSets.Values.Count);

            var translationCollection = generalTranslationSets.Values.First();
            Assert.AreEqual(2, translationCollection.SubstanceTranslationSets.Count);
            Assert.AreEqual(dimethoateRd, translationCollection.MeasuredSubstance);
            Assert.AreEqual(2, translationCollection.LinkedActiveSubstances.Count);

            Assert.AreEqual(1D, translationCollection.SubstanceTranslationSets.Sum(r => r.TranslationProportion));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => r.PositiveSubstanceConversions[dimethoate] == .5 && r.PositiveSubstanceConversions[omethoate] == .25));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => !r.PositiveSubstanceConversions.ContainsKey(dimethoate) && r.PositiveSubstanceConversions[omethoate] == .5));
        }

        /// <summary>
        /// Calculate translations sets: food specific translation sets
        /// </summary>
        [TestMethod]
        public void SubstanceTranslationSetsCalculatorTests_TestComputeFoodSpecificTranslationSets1() {
            var food = new Food() { Code = "Apple" };
            var dimethoateRd = new Compound("CompoundARD");
            var dimethoate = new Compound("CompoundA");
            var omethoate = new Compound("CompoundE");
            var substanceConversions = createMetaboliteSubstanceConversions(dimethoateRd, dimethoate, omethoate);

            // Compute without authorised uses
            var calculator = new SubstanceConversionSetsCalculator();
            var generalTranslationSets = calculator.ComputeFoodSpecificTranslationSets(food, substanceConversions.ToLookup(r => r.MeasuredSubstance));
            Assert.AreEqual(1, generalTranslationSets.Values.Count);

            var translationCollection = generalTranslationSets.Values.First();
            Assert.AreEqual(2, translationCollection.SubstanceTranslationSets.Count);
            Assert.AreEqual(dimethoateRd, translationCollection.MeasuredSubstance);
            Assert.AreEqual(2, translationCollection.LinkedActiveSubstances.Count);

            Assert.IsTrue(translationCollection.SubstanceTranslationSets.All(r => r.IsAuthorised));
            Assert.AreEqual(1D, translationCollection.SubstanceTranslationSets.Sum(r => r.TranslationProportion));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => r.PositiveSubstanceConversions[dimethoate] == .5 && r.PositiveSubstanceConversions[omethoate] == .25));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => !r.PositiveSubstanceConversions.ContainsKey(dimethoate) && r.PositiveSubstanceConversions[omethoate] == .5));
        }

        /// <summary>
        /// Calculate translations sets: food specific translation sets and authorized uses and use them
        /// </summary>
        [TestMethod]
        public void SubstanceTranslationSetsCalculatorTests_TestComputeFoodSpecificTranslationSets2() {
            var food = new Food() { Code = "Apple" };
            var dimethoateRd = new Compound("CompoundARD");
            var dimethoate = new Compound("CompoundA");
            var omethoate = new Compound("CompoundE");
            var substanceConversions = createMetaboliteSubstanceConversions(dimethoateRd, dimethoate, omethoate);
            var substanceAuthorisations = new Dictionary<(Food, Compound), SubstanceAuthorisation> {
                { (food, dimethoate), new SubstanceAuthorisation() { Food = food, Substance = dimethoate } }
            };

            // Compute with authorised uses and use them to derive translation sets
            var calculator = new SubstanceConversionSetsCalculator();
            var generalTranslationSets = calculator
                .ComputeFoodSpecificTranslationSets(food, substanceConversions.ToLookup(r => r.MeasuredSubstance), substanceAuthorisations, true);
            Assert.AreEqual(1, generalTranslationSets.Values.Count);

            var translationCollection = generalTranslationSets.Values.First();
            Assert.AreEqual(1, translationCollection.SubstanceTranslationSets.Count);
            Assert.AreEqual(dimethoateRd, translationCollection.MeasuredSubstance);
            Assert.AreEqual(2, translationCollection.LinkedActiveSubstances.Count);

            Assert.IsTrue(translationCollection.SubstanceTranslationSets.All(r => r.IsAuthorised));
            Assert.AreEqual(1D, translationCollection.SubstanceTranslationSets.Sum(r => r.TranslationProportion));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => r.PositiveSubstanceConversions[dimethoate] == .5 && r.PositiveSubstanceConversions[omethoate] == .25));
        }

        /// <summary>
        /// Calculate translations sets: food specific translation sets and authorized uses and do not use them
        /// </summary>
        [TestMethod]
        public void SubstanceTranslationSetsCalculatorTests_TestComputeFoodSpecificTranslationSets3() {
            var food = new Food() { Code = "Apple" };
            var dimethoateRd = new Compound("CompoundARD");
            var dimethoate = new Compound("CompoundA");
            var omethoate = new Compound("CompoundE");
            var substanceConversions = createMetaboliteSubstanceConversions(dimethoateRd, dimethoate, omethoate);
            var substanceAuthorisations = new Dictionary<(Food, Compound), SubstanceAuthorisation> {
                { (food, dimethoate), new SubstanceAuthorisation() { Food = food, Substance = dimethoate } }
            };

            // Compute with authorised uses, but don't use them to derive translation sets
            var calculator = new SubstanceConversionSetsCalculator();
            var generalTranslationSets = calculator
                .ComputeFoodSpecificTranslationSets(food, substanceConversions.ToLookup(r => r.MeasuredSubstance), substanceAuthorisations, false);
            Assert.AreEqual(1, generalTranslationSets.Values.Count);

            var translationCollection = generalTranslationSets.Values.First();
            Assert.AreEqual(2, translationCollection.SubstanceTranslationSets.Count);
            Assert.AreEqual(dimethoateRd, translationCollection.MeasuredSubstance);
            Assert.AreEqual(2, translationCollection.LinkedActiveSubstances.Count);

            Assert.AreEqual(1, translationCollection.SubstanceTranslationSets.Count(r => r.IsAuthorised));
            Assert.AreEqual(1D, translationCollection.SubstanceTranslationSets.Sum(r => r.TranslationProportion));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => r.PositiveSubstanceConversions[dimethoate] == .5 && r.PositiveSubstanceConversions[omethoate] == .25));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => !r.PositiveSubstanceConversions.ContainsKey(dimethoate) && r.PositiveSubstanceConversions[omethoate] == .5));
        }

        /// <summary>
        /// Calculate translations sets: food specific translation sets and authorized uses and use them but nothing is authorised
        /// </summary>
        [TestMethod]
        public void SubstanceTranslationSetsCalculatorTests_TestComputeFoodSpecificTranslationSets4() {
            var food = new Food() { Code = "Apple" };
            var dimethoateRd = new Compound("CompoundARD");
            var dimethoate = new Compound("CompoundA");
            var omethoate = new Compound("CompoundE");
            var substanceConversions = createMetaboliteSubstanceConversions(dimethoateRd, dimethoate, omethoate);
            var substanceAuthorisations = new Dictionary<(Food, Compound), SubstanceAuthorisation>();

            // Compute with authorised uses; nothing authorised
            var calculator = new SubstanceConversionSetsCalculator();
            var generalTranslationSets = calculator
                .ComputeFoodSpecificTranslationSets(food, substanceConversions.ToLookup(r => r.MeasuredSubstance), substanceAuthorisations, true);
            Assert.AreEqual(1, generalTranslationSets.Values.Count);

            var translationCollection = generalTranslationSets.Values.First();
            Assert.AreEqual(2, translationCollection.SubstanceTranslationSets.Count);
            Assert.AreEqual(dimethoateRd, translationCollection.MeasuredSubstance);
            Assert.AreEqual(2, translationCollection.LinkedActiveSubstances.Count);

            Assert.IsTrue(translationCollection.SubstanceTranslationSets.All(r => !r.IsAuthorised));
            Assert.AreEqual(1D, translationCollection.SubstanceTranslationSets.Sum(r => r.TranslationProportion));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => r.PositiveSubstanceConversions[dimethoate] == .5 && r.PositiveSubstanceConversions[omethoate] == .25));
            Assert.IsTrue(translationCollection.SubstanceTranslationSets
                .Any(r => !r.PositiveSubstanceConversions.ContainsKey(dimethoate) && r.PositiveSubstanceConversions[omethoate] == .5));
        }

        private static List<SubstanceConversion> createMetaboliteSubstanceConversions(Compound dimethoateRd, Compound dimethoate, Compound omethoate) {
            var rdDimetoate = new SubstanceConversion() {
                MeasuredSubstance = dimethoateRd,
                ActiveSubstance = dimethoate,
                ConversionFactor = 1D,
                IsExclusive = false,
                Proportion = .5
            };
            var rdOmetoate = new SubstanceConversion() {
                MeasuredSubstance = dimethoateRd,
                ActiveSubstance = omethoate,
                ConversionFactor = 0.5,
                IsExclusive = true
            };
            var substanceConversions = new List<SubstanceConversion>() { rdDimetoate, rdOmetoate };
            return substanceConversions;
        }
    }
}
