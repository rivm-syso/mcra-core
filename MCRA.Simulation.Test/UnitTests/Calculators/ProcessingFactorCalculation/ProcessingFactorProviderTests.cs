using MCRA.Data.Compiled.Objects;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// ProcessingFactorProvider calculator
    /// </summary>
    [TestClass]
    public class ProcessingFactorProviderTests {
        /// <summary>
        /// Checks whether processing types are in dictionary, if not assign arbitrary value like 0.12345 (from settingsitem interface) to processing factor
        /// Except for processing type with processing type unspecified (F28.A07XD), then assign value 1 to processing factor
        /// </summary>
        [TestMethod]
        [DataRow("", 0, 2)]
        [DataRow("Juicing", 0, 3)]
        [DataRow("Raw", 1, 4)]
        [DataRow("F28.A07XD", 2, 1)]
        [DataRow("Peeled", 3, 0.12345)]
        public void ProcessingFactorModelCollectionBuilder_TestCreateNone(
            string procType,
            int ix,
            double expected
        ) {
            var random = new McraRandomGenerator(1);
            var food = new Food { Code = "Apple" };
            var substance = new Compound() { Code = "Subst" };

            var types = new List<string>() { "Juicing", "Raw", "F28.A07XD", "Peeled" };
            var foods = new List<Food>() {
                new() { Code = $"Apple#{types[0]}"},
                new() { Code = $"Apple#{types[1]}"},
                new() { Code = $"Apple#{types[2]}"},
                new() { Code = $"Apple#CrapPie"},
            };

            var processingTypes = types.Select(c => new ProcessingType() { Code = c }).ToList();
            var selectedProcessingType = processingTypes.SingleOrDefault(c => c.Code == procType);

            var settings = new ProcessingFactorsModuleConfig { DefaultMissingProcessingFactor = 0.12345 };

            var pfModel0 = new PFFixedModel();
            pfModel0.CalculateParameters(new ProcessingFactor() {
                Compound = substance,
                FoodProcessed = foods[0],
                FoodUnprocessed = food,
                Nominal = 2,
                ProcessingType = processingTypes[0]
            });

            var pfModel1 = new PFFixedModel();
            pfModel1.CalculateParameters(new ProcessingFactor() {
                Compound = substance,
                FoodProcessed = foods[0],
                FoodUnprocessed = food,
                Nominal = 3,
                ProcessingType = processingTypes[0]
            });

            var pfModel2 = new PFFixedModel();
            pfModel2.CalculateParameters(new ProcessingFactor() {
                Compound = substance,
                FoodProcessed = foods[1],
                FoodUnprocessed = food,
                Nominal = 4,
                ProcessingType = processingTypes[1]
            });

            var processingFactor = 1D;
            var processingFactorModels = new Dictionary<(Food, Compound, ProcessingType), ProcessingFactorModel>{
                { (foods[0], substance, null), pfModel0 },
                { (foods[0], substance, processingTypes[0]), pfModel1 },
                { (foods[1], substance, processingTypes[1]), pfModel2 },
            };

            var processingFactorProvider = new ProcessingFactorProvider(
                processingFactorModels,
                settings.DefaultMissingProcessingFactor
            );
            processingFactor = processingFactorProvider.GetProcessingFactor(
                foods[ix],
                substance,
                selectedProcessingType,
                random
            );
            Assert.AreEqual(expected, processingFactor);
        }
    }
}
