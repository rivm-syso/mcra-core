using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ScenarioCaseControlFilter {

    [TestClass]
    public class StratifierTests {
        private readonly List<string> _controlScenarios = ["Welding", "Surface"];

        [TestMethod]
        public void ScenarioCaseControl_Test() {
            var scenarios = new List<OccupationalScenario>() {
                new() { Name = "Scenario1Control", Code = "Welding", CaseControl = CaseControlType.Control },
                new() { Name = "Scenario2Control", Code = "Surface" , CaseControl = CaseControlType.Case},
                new() { Name = "Scenario3Case", Code = "Chrome" , CaseControl = CaseControlType.Case}
            };
            var controls = new List<string> { "Scenario1Control", "Scenario2Control" };

            foreach (var scenario in scenarios) {
                var isControl = caseControl_Filter(scenario, p => _controlScenarios.Contains(p.Code));
                if (isControl) {
                    Assert.Contains(scenario.Name, controls);
                } else {
                    Assert.DoesNotContain(scenario.Name, controls);
                }
            }
        }

        /// <summary>
        /// Evaluates the specified occupational scenario against the provided filter predicate.
        /// </summary>
        private static bool caseControl_Filter(OccupationalScenario scenario, Predicate<OccupationalScenario> filter) {
            return filter(scenario);
        }

        [TestMethod]
        public void OccupationalScenarioStratifier_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var scenarios = new List<OccupationalScenario>() {
                new() { Name = "Welding", Code = "Welding", CaseControl = CaseControlType.Control },
                new() { Name = "Surface", Code = "Surface" , CaseControl = CaseControlType.Control},
                new() { Name = "Chrome", Code = "Chrome" , CaseControl = CaseControlType.Case}
            };

            var controlScenarioCodes = new List<string> { "Chrome" };
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

            // Create stratifiers and combine them in composite stratifier
            // Note: the order of the stratifiers in the list matters for the resulting composite stratification levels
            var stratifiers = new List<PopulationStratifier>() {
                new SimpleOccupationalScenarioStratifier(scenarios),
                new GenderStratifier(),
                new OccupationalScenarioCaseControlStratifier(scenarios, controlScenarioCodes)
            };
            var compositeStratifier = new CompositeStratifier(stratifiers);
            var dummies = simulatedIndividuals
                .Select(c => new {
                    compositeStratificationLevel = compositeStratifier.GetLevel(c) as ICompositeStratificationLevel
                })
                .ToList();

            var groups = dummies.GroupBy(g => g.compositeStratificationLevel.Parts)
                .Select(g => new {
                    StratificationLevel = g.Key,
                    Number = g.Count(),
                })
                .ToList();

            Assert.IsTrue(groups.Count == 6);
        }
    }
}
