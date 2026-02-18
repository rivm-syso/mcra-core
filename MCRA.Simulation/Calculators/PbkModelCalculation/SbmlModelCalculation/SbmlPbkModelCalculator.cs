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
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
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
            var inputMappings = inputSpecies.ToDictionary(r => r.Key, r => r.Value.Id);

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
                        r => r.Value.SubstanceAmountUnit
                    )
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

            var results = new List<PbkSimulationOutput>();
            using (var runner = new SbmlModelRunner(KineticModelInstance)) {

                // Loop over individuals
                foreach (var id in externalIndividualExposures) {

                    // Get simulation period
                    var numberOfDays = (int)getSimulationDuration(id.Individual.Age);
                    var duration = (int)(timeUnitMultiplier * numberOfDays);
                    var simulationSteps = (int)(numberOfDays * stepsPerDay) + 1;

                    // Create exposure events
                    var exposureEvents = exposureEventsGenerator
                        .CreateExposureEvents(
                            id.IndividualDayExposures,
                            routes,
                            Substance,
                            generator
                        );

                    // Collect individual properties
                    var parameters = new Dictionary<string, double>();
                    setPhysiologicalParameterValues(parameters, id.Individual);

                    // If we have positive exposures, then run simulation
                    SimulationOutput simulationOutput = null;
                    if (exposureEvents.Count > 0) {
                        simulationOutput = runner.Run(
                            exposureEvents,
                            parameters,
                            inputMappings,
                            outputStatesSelection,
                            outputTimeSeriesSelection,
                            duration,
                            simulationSteps,
                            SimulationSettings.BodyWeightCorrected
                        );
                    }

                    // Collect simulation output and add to results
                    results.Add(
                        collectPbkSimulationResults(
                            outputMappings,
                            id.Individual,
                            simulationOutput,
                            1D / stepsPerDay
                        )
                    );
                }
            }

            return results;
        }
    }
}
