using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class ExposuresAndHazardsByAgeSectionTests : SectionTestBase {

        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void HazardExposureByAgeSection_TestSummarize(RiskMetricType riskMetricType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var effect = FakeEffectsGenerator.Create(1).First();
            var substances = FakeSubstancesGenerator.Create(1);
            var referenceCompound = substances.First();
            var individuals = FakeIndividualsGenerator.Create(100, 1, random);
            FakeIndividualsGenerator.AddFakeAgeProperty(individuals, random);
            var sims = FakeIndividualsGenerator.CreateSimulated(individuals);

            var targetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerGBWPerDay, ExposureRoute.Oral);
            var hazardCharacterisationModel = FakeHazardCharacterisationModelsGenerator
                .CreateSingle(effect, referenceCompound, 1.5, targetUnit, ageDependent: true);

            var individualEffects = FakeIndividualEffectsGenerator.Create(
                sims,
                0.1,
                random,
                hazardCharacterisationModel.Value
            );

            var section = new ExposuresAndHazardsByAgeSection();
            section.Summarize(
                individualEffects,
                riskMetricType,
                targetUnit,
                hazardCharacterisationModel,
                false
            );

            AssertIsValidView(section);
            RenderView(section, filename: "TestSummarizeMoeNominal.html");
        }
    }
}
