using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.UnitDefinitions.Units;
using MCRA.Simulation.Calculators.OccupationalExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation {
    public class OccupationalExposuresCalculator {

        public static List<OccupationalScenarioExposure> Compute(
            ICollection<Compound> substances,
            ICollection<OccupationalScenario> scenarios,
            ICollection<IOccupationalTaskExposureModel> occupationalTaskExposureModels,
            List<ExposureRoute> routes,
            double nominalBodySurfaceArea,
            double fractionSkinExposed,
            double nominalVentolatoryFlowRate,
            ExposureUnitTriple targetUnit,
            bool computeSystemic,
            int seed
        ) {
            var result = new List<OccupationalScenarioExposure>();
            var taskExposuresLookup = occupationalTaskExposureModels
                .GroupBy(r => (r.Task, r.Determinants))
                .ToDictionary(
                    r => r.Key,
                    r => r.ToDictionary(r => (r.Route, r.Substance))
                );

            var ix = 0;
            foreach (var scenario in scenarios) {
                var individual = new SimulatedIndividual(new Individual(ix++), ix++);
                var taskExposures = computeScenarioTaskExposures(
                    scenario,
                    substances,
                    routes,
                    taskExposuresLookup,
                    nominalBodySurfaceArea,
                    fractionSkinExposed,
                    nominalVentolatoryFlowRate,
                    targetUnit.SubstanceAmountUnit,
                    computeSystemic: computeSystemic
                );
                var taskExposureByRouteSubstance = taskExposures.GroupBy(r => (r.Route, r.Substance));
                foreach (var taskExposure in taskExposureByRouteSubstance) {
                    if (taskExposure.Select(r => r.Unit.EstimateType).Distinct().Count() > 1) {
                        throw new Exception("Cannot combine exposure estimates of multiple estimate types");
                    }
                    var unit = taskExposure.First().Unit;
                    double exposure;
                    if (unit.EstimateType == JobTaskExposureEstimateType.TWA8h) {
                        exposure = 0D;
                        foreach (var item in taskExposure) {

                            // Correction factor for task duration (in minutes) to hours
                            var taskDurationAlignmentFactor = item.Duration / 60;

                            // Frequency correction frequency per day
                            var taskFrequencyAlignmentFactor = 1D;
                            if (computeSystemic) {
                                taskFrequencyAlignmentFactor = item.ScenarioTask.FrequencyResolution switch {
                                    FrequencyResolutionType.PerDay => item.ScenarioTask.Frequency,
                                    FrequencyResolutionType.PerWeek => item.ScenarioTask.Frequency / 7D,
                                    FrequencyResolutionType.PerMonth => item.ScenarioTask.Frequency / 30.44,
                                    _ => throw new NotImplementedException(),
                                };
                            }

                            // TWA contribution of task
                            var twaDuration = taskDurationAlignmentFactor * taskFrequencyAlignmentFactor;
                            exposure += item.Value * twaDuration / 8D;
                        }
                    } else if (unit.EstimateType == JobTaskExposureEstimateType.Undefined) {
                        exposure = taskExposure.Sum(r => r.Value);
                    } else {
                        throw new NotImplementedException();
                    }
                    var record = new OccupationalScenarioExposure() {
                        Scenario = scenario,
                        TaskExposures = [.. taskExposure],
                        Route = taskExposure.Key.Route,
                        Substance = taskExposure.Key.Substance,
                        Unit = unit,
                        Value = exposure
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        public static List<OccupationalScenarioTaskExposure> Compute(
            ICollection<Compound> substances,
            ICollection<OccupationalScenario> scenarios,
            ICollection<IOccupationalTaskExposureModel> occupationalTaskExposureModels,
            List<ExposureRoute> routes,
            double nominalBodySurfaceArea,
            double fractionSkinExposed,
            double nominalVentolatoryFlowRate,
            SubstanceAmountUnit targetAmountUnit,
            bool computeSystemic,
            int seed
        ) {
            var taskExposuresLookup = occupationalTaskExposureModels
                .GroupBy(r => (r.Task, r.Determinants))
                .ToDictionary(
                    r => r.Key,
                    r => r.ToDictionary(r => (r.Route, r.Substance))
                );

            var result = new List<OccupationalScenarioTaskExposure>();
            foreach (var scenario in scenarios) {
                var taskExposures = computeScenarioTaskExposures(
                    scenario,
                    substances,
                    routes,
                    taskExposuresLookup,
                    nominalBodySurfaceArea,
                    fractionSkinExposed,
                    nominalVentolatoryFlowRate,
                    targetAmountUnit,
                    computeSystemic
                );
                result.AddRange(taskExposures);
            }
            return result;
        }

        private static List<OccupationalScenarioTaskExposure> computeScenarioTaskExposures(
            OccupationalScenario scenario,
            ICollection<Compound> substances,
            List<ExposureRoute> routes,
            Dictionary<(OccupationalTask Task, OccupationalTaskDeterminants Determinants), Dictionary<(ExposureRoute Route, Compound Substance), IOccupationalTaskExposureModel>> taskExposuresLookup,
            double nominalBodySurfaceArea,
            double fractionSkinExposed,
            double nominalVentolatoryFlowRate,
            SubstanceAmountUnit targetAmountUnit,
            bool computeSystemic
        ) {
            var result = new List<OccupationalScenarioTaskExposure>();
            var scenarioTasks = scenario.Tasks;
            foreach (var task in scenarioTasks) {
                if (taskExposuresLookup.TryGetValue((task.OccupationalTask, task.Determinants()), out var taskExposureModels)) {
                    foreach (var route in routes) {
                        foreach (var substance in substances) {
                            if (taskExposureModels.TryGetValue((route, substance), out var model)) {
                                var taskExposure = computeScenarioTaskExposure(
                                    scenario,
                                    task,
                                    route,
                                    substance,
                                    model,
                                    nominalBodySurfaceArea,
                                    fractionSkinExposed,
                                    nominalVentolatoryFlowRate,
                                    targetAmountUnit,
                                    computeSystemic
                                );
                                result.Add(taskExposure);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static OccupationalScenarioTaskExposure computeScenarioTaskExposure(
            OccupationalScenario scenario,
            OccupationalScenarioTask task,
            ExposureRoute route,
            Compound substance,
            IOccupationalTaskExposureModel model,
            double bodySurfaceArea,
            double fractionSkinExposed,
            double ventilatoryFlowRate,
            SubstanceAmountUnit targetAmountUnit,
            bool computeSystemic
        ) {
            var unit = new OccupationalExposureUnit(
                targetAmountUnit,
                computeSystemic ? JobTaskExposureUnitDenominator.None : model.Unit.Denominator,
                computeSystemic ? JobTaskExposureEstimateType.Undefined : model.Unit.EstimateType,
                computeSystemic ? TimeUnit.Days : model.Unit.TimeUnit
            );

            // Get alignment factor for substance amount unit
            var amountUnitAlignmentFactor = model.Unit.SubstanceAmountUnit
                .GetMultiplicationFactor(targetAmountUnit);

            // Use task duration to get a per-task estimate
            var taskDurationAlignmentFactor = model.Unit.TimeUnit switch {
                TimeUnit.NotSpecified => 1D,
                TimeUnit.Hours => task.Duration / 60,
                TimeUnit.Days => task.Duration / 60 / 24,
                _ => throw new NotImplementedException(),
            };

            // Frequency correction
            var taskFrequencyAlignmentFactor = 1D;
            if (computeSystemic) {
                taskFrequencyAlignmentFactor = task.FrequencyResolution switch {
                    FrequencyResolutionType.PerDay => task.Frequency,
                    FrequencyResolutionType.PerWeek => task.Frequency / 7D,
                    FrequencyResolutionType.PerMonth => task.Frequency / 30.44,
                    _ => throw new NotImplementedException(),
                };
            }

            // Exposure unit denominator alignment factor
            var denominatorAlignmentFactor = 1D;
            if (computeSystemic) {
                if (route == ExposureRoute.Dermal) {
                    if (model.Unit.Denominator == JobTaskExposureUnitDenominator.SquareCentimeters) {
                        denominatorAlignmentFactor = bodySurfaceArea * fractionSkinExposed;
                    } else {
                        throw new NotImplementedException();
                    }
                }
                if (route == ExposureRoute.Inhalation) {
                    if (model.Unit.Denominator == JobTaskExposureUnitDenominator.CubicMeters) {
                        // Align duration (minutes) with ventilatory flow rate (per day)
                        taskDurationAlignmentFactor *= task.Duration / 60 / 24;
                        denominatorAlignmentFactor = ventilatoryFlowRate;
                    } else {
                        throw new NotImplementedException();
                    }
                }
            }

            var result = new OccupationalScenarioTaskExposure() {
                Scenario = scenario,
                ScenarioTask = task,
                Route = route,
                Substance = substance,
                Unit = unit,
                Value = model.GetNominal()
                    * amountUnitAlignmentFactor
                    * denominatorAlignmentFactor
                    * taskDurationAlignmentFactor
                    * taskFrequencyAlignmentFactor
            };
            return result;
        }
    }
}
