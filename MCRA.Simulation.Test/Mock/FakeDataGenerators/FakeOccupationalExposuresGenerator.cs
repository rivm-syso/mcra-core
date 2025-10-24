using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    public static class FakeOccupationalExposuresGenerator {

        public static List<OccupationalScenario> CreateScenarios(
            int[] taskCombinations,
            IRandom random
        ) {
            var result = new List<OccupationalScenario>();
            for (int i = 0; i < taskCombinations.Length; i++) {
                var tasks = Enumerable
                    .Range(0, taskCombinations[i])
                    .Select(r => new OccupationalTask() {
                        Code = $"Task_{i}_{r}",
                        Name = $"Task ({i}:{r})",
                        Description = $"Task ({i}:{r})"
                    })
                    .ToList();
                var scenarioTaskLists = createScenarioTasksRecursive(tasks, random);
                for (int j = 0; j < scenarioTaskLists.Count; j++) {
                    var scenario = new OccupationalScenario() {
                        Code = $"Scenario_{i}{(char)(j + 97)}",
                        Name = $"Scenario {i}{(char)(j + 97)}",
                        Description = $"Scenario {i}{(char)(j + 97)}",
                        Tasks = scenarioTaskLists[j]
                    };
                    result.Add(scenario);
                }
            }
            return result;
        }

        public static List<OccupationalTaskExposure> CreateExposures(
            ICollection<(OccupationalTask Task, OccupationalTaskDeterminants Determinants)> tasks,
            ICollection<ExposureRoute> routes,
            ICollection<Compound> substances,
            double[] percentiles,
            IRandom random
        ) {
            var result = new List<OccupationalTaskExposure>();
            var routeUnits = new Dictionary<ExposureRoute, List<(JobTaskExposureUnit Unit, JobTaskExposureEstimateType EstimateType)>>() {
                { ExposureRoute.Inhalation, [
                    (JobTaskExposureUnit.ugPerM3, JobTaskExposureEstimateType.TWA8h),
                    (JobTaskExposureUnit.mgPerM3, JobTaskExposureEstimateType.TWA8h)
                ]},
                { ExposureRoute.Dermal, [
                    (JobTaskExposureUnit.ugPerCm2PerDay, JobTaskExposureEstimateType.Undefined),
                    (JobTaskExposureUnit.mgPerCm2PerDay, JobTaskExposureEstimateType.Undefined)
                ]}
            };
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    foreach (var item in tasks) {
                        var mean = random.NextDouble(1, 5);
                        var upper = mean + random.NextDouble(1, 5);
                        var distribution = LogNormalDistribution.FromMeanAndUpper(mean, upper);
                        var (unit, estimateType) = routeUnits[route][random.Next(routeUnits[route].Count - 1)];
                        var records = percentiles
                            .Select(r => new OccupationalTaskExposure() {
                                OccupationalTask = item.Task,
                                Substance = substance,
                                ExposureRoute = route,
                                RpeType = item.Determinants.RPEType,
                                Percentage = r,
                                Value = distribution.InvCDF(r/100D),
                                Unit = unit,
                                EstimateType = estimateType
                            })
                            .ToList();
                        result.AddRange(records);
                    }
                }
            }
            return result;
        }

        private static List<List<OccupationalScenarioTask>> createScenarioTasksRecursive(
            IEnumerable<OccupationalTask> tasks,
            IRandom random
        ) {
            var durations = new double[] { 60, 120, 240, 300, 480 };
            var frequencies = new (int, FrequencyResolutionType)[] {
                (1, FrequencyResolutionType.PerDay),
                (1, FrequencyResolutionType.PerWeek),
                (5, FrequencyResolutionType.PerWeek),
                (1, FrequencyResolutionType.PerMonth),
            };
            var result = new List<List<OccupationalScenarioTask>>();
            if (tasks.Any()) {
                var task = tasks.First();
                var partialResult = tasks.Count() == 1
                    ? [[]]
                    : createScenarioTasksRecursive(tasks.Skip(1), random);
                var determinantCombinations = createDeterminantCombinations(random);
                foreach (var combination in determinantCombinations) {
                    var duration = durations[random.Next(durations.Length)];
                    var frequency = frequencies[random.Next(frequencies.Length)];
                    foreach (var partialResultRecord in partialResult) {
                        var record = new OccupationalScenarioTask() {
                            OccupationalTask = task,
                            RpeType = combination.RPEType,
                            Duration = duration,
                            Frequency = frequency.Item1,
                            FrequencyResolution = frequency.Item2,
                        };
                        var resultRecord = new List<OccupationalScenarioTask> { record };
                        resultRecord.AddRange(partialResultRecord);
                        result.Add(resultRecord);
                    }
                }
            }
            return result;
        }

        private static List<OccupationalTaskDeterminants> createDeterminantCombinations(
            IRandom random
        ) {
            if (random.NextDouble() > .5) {
                return [
                    new() { RPEType = RPEType.None },
                    new() { RPEType = RPEType.RPE },
                ];
            } else {
                return [
                    new() { RPEType = RPEType.Undefined }
                ];
            }
        }
    }
}
