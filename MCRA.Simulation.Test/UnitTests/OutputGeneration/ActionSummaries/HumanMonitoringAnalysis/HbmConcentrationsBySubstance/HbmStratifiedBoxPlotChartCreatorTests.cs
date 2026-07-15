using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis {

    [TestClass]
    public class HbmStratifiedBoxPlotChartCreatorTests : ChartCreatorTestBase {

        private readonly List<OccupationalScenario> _scenarios = [
            new() { Name = "Welding", Code = "Welding", CaseControl = CaseControlType.Control },
            new() { Name = "Surface", Code = "Surface" , CaseControl = CaseControlType.Control},
            new() { Name = "Chrome", Code = "Chrome" , CaseControl = CaseControlType.Case}
        ];

        private readonly List<string> _controlScenarioCodes = ["Chrome"];

        [TestMethod]
        public void HbmStratifiedBoxPlotChartCreator_TestCaseControl() {
            var collection = getConcentrations(_scenarios);
            var stratifier = new PopulationStratifier([
                new OccupationalCaseControlStratifier(_scenarios, _controlScenarioCodes)
            ]);
            var section = new HbmCumulativeIndividualDistributionsSection();
            section.Summarize(collection, stratifier, 5, 95, false);
            var exposureTarget = new ExposureTarget(BiologicalMatrix.Blood);
            var records = section.StratifiedHbmBoxPlotRecords[exposureTarget];
            var chartCreator = new HbmStratifiedBoxPlotChartCreator<SubstanceContributorKey, HbmSubstancePercentilesRecord>(
                records,
                exposureTarget,
                section.SectionId,
                "mg/L",
                false,
                section.StratificationVariables
            );
            RenderChart(chartCreator, "TestCaseControl");
        }

        [TestMethod]
        public void HbmStratifiedBoxPlotChartCreator_TestGenderThenOccupational() {
            var collection = getConcentrations(_scenarios);
            var stratifier = new PopulationStratifier([
                new GenderStratifier(),
                new OccupationalScenarioStratifier(_scenarios),
                new OccupationalCaseControlStratifier(_scenarios, _controlScenarioCodes),
            ]);
            var section = new HbmCumulativeIndividualDistributionsSection();
            section.Summarize(collection, stratifier, 5, 95, false);
            var exposureTarget = new ExposureTarget(BiologicalMatrix.Blood);
            var records = section.StratifiedHbmBoxPlotRecords[exposureTarget];
            var chartCreator = new HbmStratifiedBoxPlotChartCreator<SubstanceContributorKey, HbmSubstancePercentilesRecord>(
                records,
                exposureTarget,
                section.SectionId,
                "mg/L",
                false,
                section.StratificationVariables
            );
            RenderChart(chartCreator, "TestGenderThenOccupational");
        }

        [TestMethod]
        public void HbmStratifiedBoxPlotChartCreator_TestOccupationalThenGender() {
            var collection = getConcentrations(_scenarios);
            var stratifier = new PopulationStratifier([
                new OccupationalScenarioStratifier(_scenarios),
                new OccupationalCaseControlStratifier(_scenarios, _controlScenarioCodes),
                new GenderStratifier()
            ]);
            var section = new HbmCumulativeIndividualDistributionsSection();
            section.Summarize(collection, stratifier, 5, 95, false);
            var exposureTarget = new ExposureTarget(BiologicalMatrix.Blood);
            var records = section.StratifiedHbmBoxPlotRecords[exposureTarget];
            var chartCreator = new HbmStratifiedBoxPlotChartCreator<SubstanceContributorKey, HbmSubstancePercentilesRecord>(
                records,
                exposureTarget,
                section.SectionId,
                "mg/L",
                false,
                section.StratificationVariables
            );
            RenderChart(chartCreator, "TestOccupationalThenGender");
        }

        private static HbmCumulativeIndividualCollection getConcentrations(List<OccupationalScenario> scenarios) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(100, 2, random);
            FakeIndividualsGenerator.AddFakeSexProperty(individuals, random);
            var simulatedIndividuals = new List<SimulatedIndividual>();
            var id = 0;

            foreach (var individual in individuals) {
                var sc = random.Next(scenarios.Count);
                var simIndividual = new SimulatedIndividual(individual, individual.Id) { OccupationalScenario = scenarios[sc] };
                simulatedIndividuals.Add(simIndividual);
                id++;
            }
            var collection = FakeHbmCumulativeIndividualConcentrationsGenerator
                .Create(simulatedIndividuals, random);
            return collection;
        }
    }
}
