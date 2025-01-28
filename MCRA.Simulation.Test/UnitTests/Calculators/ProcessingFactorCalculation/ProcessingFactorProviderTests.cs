using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorProvider tests.
    /// </summary>
    [TestClass]
    public class ProcessingFactorProviderTests {

        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestDrawGeneric() {
            var random = new McraRandomGenerator(1);

            var substances = FakeSubstancesGenerator.Create(2);
            var food = new Food("APPLE");
            var processingType = new ProcessingType("JUICING");

            // Substance-specific PF for first substance
            var pfSpecific = 2;
            var pfModel0 = new PFFixedModel(
                new ProcessingFactor() {
                    FoodUnprocessed = food,
                    Compound = substances[0],
                    ProcessingType = processingType,
                    Nominal = pfSpecific
                }
            );
            pfModel0.CalculateParameters();
            
            // Generic PF
            var pfGeneric = 4;
            var pfModel1 = new PFFixedModel(
                new ProcessingFactor() {
                    FoodUnprocessed = food,
                    ProcessingType = processingType,
                    Nominal = pfGeneric
                }
            );
            pfModel1.CalculateParameters();

            // Create processing factor model provider
            var processingFactorProvider = new ProcessingFactorProvider(
                [pfModel0, pfModel1],
                defaultMissingProcessingFactor: 0.12345
            );

            // Draw/expect substance-specific PF for first substance
            var pf = processingFactorProvider.GetProcessingFactor(food, substances[0], processingType, random);
            Assert.AreEqual(pfSpecific, pf);

            // Draw/expect generic PF for second substance
            pf = processingFactorProvider.GetProcessingFactor(food, substances[1], processingType, random);
            Assert.AreEqual(pfGeneric, pf);
        }

        [TestMethod]
        public void ProcessingFactorModelCollectionBuilder_TestDrawExist() {
            var random = new McraRandomGenerator(1);

            var substance = new Compound("CMPX");
            var food = new Food("APPLE");
            var processingType = new ProcessingType("JUICING");

            // Create processing factor model
            var pfModel0 = new PFFixedModel(
                new ProcessingFactor() {
                    FoodUnprocessed = food,
                    Compound = substance,
                    ProcessingType = processingType,
                    Nominal = 2
                }
            );
            pfModel0.CalculateParameters();

            // Create processing factor model provider
            var processingFactorProvider = new ProcessingFactorProvider(
                [pfModel0],
                defaultMissingProcessingFactor: 0.12345
            );

            // Draw processing factor
            var pf = processingFactorProvider.GetProcessingFactor(food, substance, processingType, random);
            Assert.AreEqual(2, pf);
        }

        [TestMethod]
        [DataRow("JUICING", true)]
        [DataRow("F28.A07XD", false)]
        public void ProcessingFactorModelCollectionBuilder_TestDrawMissing(
            string processingTypeCode,
            bool expectDefault
        ) {
            var random = new McraRandomGenerator(1);

            var substance = new Compound("CMPX");
            var food = new Food("APPLE");
            var processingType = new ProcessingType(processingTypeCode);

            // Create processing factor provider with specific default for missing
            var defaultMissingProcessingFactor = 0.12345;
            var processingFactorProvider = new ProcessingFactorProvider(
                [],
                defaultMissingProcessingFactor
            );

            // Draw processing factor
            var pf = processingFactorProvider.GetProcessingFactor(food, substance, processingType, random);

            // Assert: expect default for all processing types, except special processing
            // type "unspecified" (F28.A07XD).
            var expected = expectDefault ? defaultMissingProcessingFactor : 1D;
            Assert.AreEqual(pf, expected);
        }
    }
}
