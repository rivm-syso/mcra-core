using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HazardCharacterisations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, HazardCharacterisations
    /// </summary>
    [TestClass]
    public class AvailableHazardCharacterisationsChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart available target dose and test AvailableHazardCharacterisationsSummarySection view
        /// </summary>
        [TestMethod]
        public void AvailableHazardCharacterisationsChartCreator_TestCreate() {
            var substances = Enumerable.Range(1, 10).Select(r => $"Compound {r}").ToList();
            var records = substances.Select((r, ix) => new AvailableHazardCharacterisationsSummaryRecord() {
                CompoundCode = r,
                CompoundName = r,
                SystemHazardCharacterisation = Math.Pow(10, ix - 5),
                CriticalEffectSize = 0.05,
                EffectCode = "code",
                EffectName = "name",
                ExposureRoute = "oral",
                ExpressionTypeConversionFactor = 1,
                HazardCharacterisation = 1,
                ModelCode = "xx",
                ModelEquation = "equation",
                ModelParameterValues = "a,b,c",
                NominalInterSpeciesConversionFactor = 1,
                NominalIntraSpeciesConversionFactor = 1,
                NominalKineticConversionFactor = 1,
                Organ = "Liver",
                PotencyOrigin = "unkown",
                Species = "rat"
            }).ToList();

            var targetUnit = new TargetUnit(ExposureTarget.DietaryExposureTarget, SubstanceAmountUnit.Milligrams, ConcentrationMassUnit.Kilograms);
            var section = new AvailableHazardCharacterisationsSummarySection() {
                ChartRecords = new () { { targetUnit, records } },
            };
            var chart = new AvailableHazardCharacterisationsChartCreator(section.SectionId, records, "unit");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
