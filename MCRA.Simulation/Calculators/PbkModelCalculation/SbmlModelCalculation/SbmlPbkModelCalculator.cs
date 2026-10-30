using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Sbml.BatchSimulation;
using MCRA.Utils.Sbml.Simulation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation {
    public sealed class SbmlPbkModelCalculator : PbkModelCalculatorBase<SbmlPbkModelSpecification> {
        public SbmlPbkModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings,
            Dictionary<ExposureRoute, double> absorptionFactors = null
        ) : base(kineticModelInstance, simulationSettings, absorptionFactors) {
        }

        public override List<PbkSimulationOutput> calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            IRandom generator
        ) {
            // Get time resolution
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(PbkModelSpecification.Resolution);
            var stepsPerDay = getSimulationStepsPerDay();
            var stepLength = 1d / stepsPerDay * timeUnitMultiplier;

            // Get bodyweight parameter
            var bwParam = PbkModelSpecification.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight);

            // Get model input mappings for exposure routes
            var inputSpecies = PbkModelSpecification.GetRouteInputSpecies(SimulationSettings.AllowFallbackSystemic);
            var inputMappings = inputSpecies.ToDictionary(r => r.Key, r => r.Value.Species.Id);

            // Get absorption factors for routes using systemic fallback
            var absorptionFactors = SimulationSettings.AllowFallbackSystemic
                ? routes.ToDictionary(r => r, r => inputSpecies.TryGetValue(r, out var species) && species.IsFallback ? AbsorptionFactors[r] : 1)
                : null;

            // Check if routes are supported by the model
            var missingRoutes = routes.Except(inputMappings.Keys).ToList();
            if (missingRoutes.Count > 0) {
                var routeNames = string.Join(", ", missingRoutes.Select(r => r.GetDisplayName()));
                throw new Exception($"Exposure routes {routeNames} are not supported by PBK model {PbkModelSpecification.Id}.");
            }

            // Initialise exposure events generator
            var exposureEventsGenerator = new ExposureEventsGenerator(
                SimulationSettings,
                PbkModelSpecification.Resolution,
                exposureUnit,
                inputSpecies
                    .ToDictionary(
                        r => r.Key,
                        r => r.Value.Species.SubstanceAmountUnit
                    ),
                absorptionFactors
            );

            // Get model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            // The bracket notation '[..]' indicates roadrunner to return outputs as concentrations
            var outputTimeSeriesSelection = outputMappings
                .Select(r => r.TargetUnit.IsPerBodyWeight ? $"[{r.OutputId}]" : r.OutputId)
                .ToList();

            // For lifetime modelling, include output timeseries for body weight and age
            if (PbkModelSpecification.IsLifetimeModel()) {
                if (bwParam != null && bwParam.IsInternalParameter) {
                    outputTimeSeriesSelection.Add(bwParam.Id);
                }
                var ageParam = PbkModelSpecification.GetParameterDefinitionByType(PbkModelParameterType.Age);
                if (ageParam != null && ageParam.IsInternalParameter) {
                    outputTimeSeriesSelection.Add(ageParam.Id);
                }
            }

            // For the compartments, we want to get the final value at the end of the simulation
            var outputStatesSelection = outputMappings
                .Select(r => r.CompartmentId)
                .ToList();

            // Create simulations
            var simulationInputs = new List<SbmlSimulationJob>();
            for (int i = 0; i < externalIndividualExposures.Count; i++) {
                var ind = externalIndividualExposures.ElementAt(i);
                var numberOfDays = (int)getSimulationDuration(ind.Individual.Age);
                var duration = (int)(timeUnitMultiplier * numberOfDays);
                var simulationSteps = (int)(numberOfDays * stepsPerDay) + 1;

                // Create exposure events
                var exposureEvents = exposureEventsGenerator
                    .CreateExposureEvents(ind.IndividualDayExposures, routes, Substance, generator)
                    .Select(r => toSbmlSimulationEvent(
                        r,
                        inputMappings,
                        SimulationSettings.ApplyBodyWeightScaling,
                        bwParam.Id
                    ))
                    .ToList();

                // Create simulation parameters
                var parameters = new Dictionary<string, double>();
                foreach (var parameter in PbkModelSpecification.GetParameters()) {
                    if (KineticModelInstance.KineticModelInstanceParameters.TryGetValue(parameter.Id, out var value)) {
                        parameters.Add(parameter.Id, value.Value);
                    }
                }
                setPhysiologicalParameterValues(parameters, ind.Individual);

                if (exposureEvents.Count > 0) {
                    var job = new SbmlSimulationJob {
                        JobId = i.ToString(),
                        Parameters = parameters,
                        Events = exposureEvents,
                        OutputStates = outputStatesSelection,
                        OutputTimeSeries = outputTimeSeriesSelection,
                        Duration = duration,
                        Steps = simulationSteps
                    };
                    simulationInputs.Add(job);
                }
            }

            // Create SBML batch simulation runner and run
            var sbmlBatchJobRunner = new SbmlBatchSimulationRunner();
            var outputs = sbmlBatchJobRunner
                .Compute(
                    filename: PbkModelSpecification.FileName,
                    jobs: simulationInputs,
                    proxyType: Simulation.RoadRunnerOptions.ProxyType,
                    maxParallelWorkers: Simulation.RoadRunnerOptions.GetEffectiveWorkerCount(),
                    showConsoleWindow: Simulation.RoadRunnerOptions.ShowConsoleWindow,
                    extensiveConsoleLogging: Simulation.RoadRunnerOptions.ExtensiveConsoleLogging
                )
                .ToDictionary(r => r.JobId);

            // Process simulation outputs
            var results = new List<PbkSimulationOutput>(simulationInputs.Count);
            for (int i = 0; i < externalIndividualExposures.Count; i++) {
                var ind = externalIndividualExposures.ElementAt(i);
                var jobId = i.ToString();
                var simulationOutput = outputs.TryGetValue(jobId, out var jobResult)
                    ? new SimulationOutput {
                        Time = jobResult.Result.Time,
                        OutputTimeSeries = jobResult.Result.OutputTimeSeries,
                        OutputStates = jobResult.Result.OutputStates
                    }
                    : null;

                var pbkSimulationOutput = collectPbkSimulationResults(
                    outputMappings,
                    ind.Individual,
                    simulationOutput,
                    1D / stepsPerDay
                );
                results.Add(pbkSimulationOutput);
            }

            return results;
        }

        private static SbmlSimulationEvent toSbmlSimulationEvent(
            IExposureEvent exposureEvent,
            Dictionary<ExposureRoute, string> targetMappings,
            bool bodyweightAdjustment,
            string bodyweightParameter
        ) => exposureEvent switch {
            SingleExposureEvent single => new SbmlSimulationEvent {
                Id = $"single_{exposureEvent.Route}_{exposureEvent.Time}",
                Kind = SbmlSimulationEventType.Single,
                Target = targetMappings[exposureEvent.Route],
                Time = single.Time,
                Value = single.Value,
                Adjustment = bodyweightAdjustment ? $"*{bodyweightParameter}" : null
            },
            RepeatingExposureEvent repeating => new SbmlSimulationEvent {
                Id = $"repeating_{exposureEvent.Route}_{exposureEvent.Time}",
                Kind = SbmlSimulationEventType.Repeated,
                Target = targetMappings[exposureEvent.Route],
                Time = repeating.Time,
                Interval = repeating.Interval,
                TimeEnd = repeating.TimeEnd,
                Value = repeating.Value,
                Adjustment = bodyweightAdjustment ? $"*{bodyweightParameter}" : null
            },
            _ => throw new NotSupportedException($"Unsupported exposure event type {exposureEvent.GetType().Name}.")
        };
    }
}
