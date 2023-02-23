using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntraSpeciesModels {

    /// <summary>
    /// IntraSpeciesFactorsExtensionTests
    /// </summary>
    [TestClass]
    public class IntraSpeciesFactorsExtensionTests {

        /// <summary>
        /// Test retreive null model from null-model-only collection.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorExtensions_TestGetNullModelCollection() {
            var collection = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();
            var nullModel = new IntraSpeciesFactorModel();
            collection.Add((null, null), nullModel);

            var model = collection.Get(new Effect(), new Compound());
            Assert.AreEqual(nullModel, model);

            model = collection.Get(null, null);
            Assert.AreEqual(nullModel, model);
        }

        /// <summary>
        /// Test retreive model from collectyion with null-model-only and effect/substance
        /// specific model.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorExtensions_TestGetBothSpecific() {
            var collection = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();

            // Default model
            var nullModel = new IntraSpeciesFactorModel();
            collection.Add((null, null), nullModel);

            // Specific model
            var specificModel = new IntraSpeciesFactorModel();
            var effect = new Effect();
            var substance = new Compound();
            collection.Add((effect, substance), specificModel);

            // Test for new effect and new substance: expect null model
            var model = collection.Get(new Effect(), new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for null effect and null substance: expect null model
            model = collection.Get(null, null);
            Assert.AreEqual(nullModel, model);

            // Test for specific effect and new substance: expect null model
            model = collection.Get(effect, new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for specific effect and null substance: expect null model
            model = collection.Get(effect, null);
            Assert.AreEqual(nullModel, model);

            // Test for new effect and specific substance: expect null model
            model = collection.Get(new Effect(), substance);
            Assert.AreEqual(nullModel, model);

            // Test for null effect and specific substance: expect null model
            model = collection.Get(null, substance);
            Assert.AreEqual(nullModel, model);

            // Test for specific effect and specific substance: expect specific model
            model = collection.Get(effect, substance);
            Assert.AreEqual(specificModel, model);
        }

        /// <summary>
        /// Test retreive model from collectyion with null-model-only and effect
        /// specific model.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorExtensions_TestGetEffectSpecific() {
            var collection = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();

            // Default model
            var nullModel = new IntraSpeciesFactorModel();
            collection.Add((null, null), nullModel);

            // Specific model
            var specificModel = new IntraSpeciesFactorModel();
            var effect = new Effect();
            collection.Add((effect, null), specificModel);

            // Test for new effect and new substance: expect null model
            var model = collection.Get(new Effect(), new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for new effect and null substance: expect null model
            model = collection.Get(new Effect(), null);
            Assert.AreEqual(nullModel, model);

            // Test for null effect and new substance: expect null model
            model = collection.Get(null, new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for null effect and null substance: expect null model
            model = collection.Get(null, null);
            Assert.AreEqual(nullModel, model);

            // Test for specific effect and new substance: expect specific model
            model = collection.Get(effect, new Compound());
            Assert.AreEqual(specificModel, model);

            // Test for specific effect and null substance: expect specific model
            model = collection.Get(effect, null);
            Assert.AreEqual(specificModel, model);
        }

        /// <summary>
        /// Test retreive model from collectyion with null-model-only and substance
        /// specific model.
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorExtensions_TestGetSubstanceSpecific() {
            var collection = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();

            // Default model
            var nullModel = new IntraSpeciesFactorModel();
            collection.Add((null, null), nullModel);

            // Specific model
            var specificModel = new IntraSpeciesFactorModel();
            var substance = new Compound();
            collection.Add((null, substance), specificModel);

            // Test for new effect and new substance: expect null model
            var model = collection.Get(new Effect(), new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for null effect and new substance: expect null model
            model = collection.Get(null, new Compound());
            Assert.AreEqual(nullModel, model);

            // Test for new effect and null substance: expect null model
            model = collection.Get(new Effect(), null);
            Assert.AreEqual(nullModel, model);

            // Test for null effect and null substance: expect null model
            model = collection.Get(null, null);
            Assert.AreEqual(nullModel, model);

            // Test for new effect and specific substance: expect null model
            model = collection.Get(new Effect(), substance);
            Assert.AreEqual(specificModel, model);

            // Test for null effect and specific substance: expect null model
            model = collection.Get(null, substance);
            Assert.AreEqual(specificModel, model);
        }
    }
}
