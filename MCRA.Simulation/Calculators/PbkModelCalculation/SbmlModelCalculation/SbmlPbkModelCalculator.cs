using System.Globalization;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.ExposureEventsGeneration;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
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
                var bwParam = PbkModelSpecification.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight);
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

            var work = new List<(SimulatedIndividual individual, List<IExposureEvent> events, Dictionary<string, double> parameters, int duration, int steps)>();
            foreach (var id in externalIndividualExposures) {
                var numberOfDays = (int)getSimulationDuration(id.Individual.Age);
                var duration = (int)(timeUnitMultiplier * numberOfDays);
                var simulationSteps = (int)(numberOfDays * stepsPerDay) + 1;
                var exposureEvents = exposureEventsGenerator.CreateExposureEvents(
                    id.IndividualDayExposures, routes, Substance, generator);
                var parameters = new Dictionary<string, double>();
                foreach (var parameter in PbkModelSpecification.GetParameters()) {
                    if (KineticModelInstance.KineticModelInstanceParameters.TryGetValue(parameter.Id, out var value))
                        parameters.Add(parameter.Id, value.Value);
                }
                setPhysiologicalParameterValues(parameters, id.Individual);
                work.Add((id.Individual, exposureEvents, parameters, duration, simulationSteps));
            }

            var results = new List<PbkSimulationOutput>(work.Count);

            var workerCount = Simulation.RoadRunnerOptions.GetEffectiveWorkerCount();
            var roadRunnerWorkerProxyType = Simulation.RoadRunnerOptions.ProxyType;

            var proxyFactory = new RoadRunnerWorkerProxyFactory(workerCount: workerCount);

            var ss = System.Diagnostics.Stopwatch.StartNew();

            // Create at most one worker proxy per unit of work, but never more than the number of
            // workers recommended by the factory, so the total workload (e.g. all individuals) is
            // divided over the configured number of workers/processes.
            var proxies = new List<IRoadRunnerWorkerProxy>(workerCount);
            try {
                for (var w = 0; w < workerCount; w++) {
                    proxies.Add(proxyFactory.Create(
                        roadRunnerWorkerProxyType, PbkModelSpecification, KineticModelInstance, inputMappings));
                }

                // Partition the work round-robin across the proxies: each proxy processes its own
                // slice of individuals sequentially, while the slices run concurrently with each other.
                var outputs = new RoadRunnerSimulationResult[work.Count];
                var partitionTasks = new Task[proxies.Count];
                for (var w = 0; w < proxies.Count; w++) {
                    var proxy = proxies[w];
                    var startIndex = w;
                    partitionTasks[w] = runPartitionAsync(
                        proxy, startIndex, proxies.Count, work, outputs, outputStatesSelection, outputTimeSeriesSelection,
                        SimulationSettings.ApplyBodyWeightScaling);
                }
                Task.WhenAll(partitionTasks).GetAwaiter().GetResult();

                for (var i = 0; i < work.Count; i++) {
                    var (individual, events, parameters, duration, steps) = work[i];
                    var output = outputs[i];
                    SimulationOutput simulationOutput = output == null ? null : new SimulationOutput {
                        Time = output.Time,
                        OutputTimeSeries = output.OutputTimeSeries,
                        OutputStates = output.OutputStates
                    };
                    results.Add(collectPbkSimulationResults(outputMappings, individual,
                        simulationOutput, 1D / stepsPerDay));
                }

                ss.Stop();
                var elapsed = ss.Elapsed.ToString();
                Console.WriteLine();
                Console.WriteLine($"PBK model {PbkModelSpecification.Id} simulation completed in {elapsed}.");
                return results;
            } catch (Exception ex) {
                var individualId = ex.Data["JobId"] is string jobId
                    && int.TryParse(jobId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedJobId)
                    ? parsedJobId
                    : 0;
                var individualCode = individualId >= 0 && individualId < work.Count
                    ? work[individualId].individual.Individual.ToString()
                    : "unknown";
                throw new InvalidOperationException($"PBK model {PbkModelSpecification.Id} simulation failed for individual {individualCode}", ex);
            } finally {
                foreach (var proxy in proxies) {
                    proxy.Dispose();
                }
            }
        }

        private static async Task runPartitionAsync(
            IRoadRunnerWorkerProxy proxy,
            int startIndex,
            int stride,
            List<(SimulatedIndividual individual, List<IExposureEvent> events, Dictionary<string, double> parameters, int duration, int steps)> work,
            RoadRunnerSimulationResult[] outputs,
            List<string> outputStatesSelection,
            List<string> outputTimeSeriesSelection,
            bool applyBodyWeightScaling
        ) {
            // Collect all jobs for this partition first, so the whole partition is submitted to the
            // proxy/worker in a single batched call instead of one call per individual. For an
            // out-of-process worker, this reduces e.g. 144 individuals to 1 HTTP round-trip per
            // worker (instead of 144), while still reusing the same loaded model for all of them.
            var indices = new List<int>();
            var jobs = new List<RoadRunnerSimulationJob>();
            for (var i = startIndex; i < work.Count; i += stride) {
                var (individual, events, parameters, duration, steps) = work[i];
                if (events.Count == 0) {
                    continue;
                }
                indices.Add(i);
                jobs.Add(new RoadRunnerSimulationJob {
                    JobId = i.ToString(CultureInfo.InvariantCulture),
                    Parameters = parameters,
                    Events = [.. events.Select(toRoadRunnerEvent)],
                    OutputStates = outputStatesSelection,
                    OutputTimeSeries = outputTimeSeriesSelection,
                    Duration = duration,
                    Steps = steps,
                    ApplyBodyWeightScaling = applyBodyWeightScaling
                });
            }

            if (jobs.Count == 0) {
                return;
            }

            var results = await proxy.RunAsync(jobs).ConfigureAwait(false);
            for (var j = 0; j < indices.Count; j++) {
                outputs[indices[j]] = results[j];
            }
        }

        private static RoadRunnerExposureEvent toRoadRunnerEvent(IExposureEvent exposureEvent) => exposureEvent switch {
            SingleExposureEvent single => new RoadRunnerExposureEvent {
                Kind = "single",
                Route = single.Route.ToString(),
                Time = single.Time,
                Value = single.Value
            },
            RepeatingExposureEvent repeating => new RoadRunnerExposureEvent {
                Kind = "repeating",
                Route = repeating.Route.ToString(),
                Time = repeating.Time,
                Interval = repeating.Interval,
                TimeEnd = repeating.TimeEnd,
                Value = repeating.Value
            },
            _ => throw new NotSupportedException($"Unsupported exposure event type {exposureEvent.GetType().Name}.")
        };

    }
}
