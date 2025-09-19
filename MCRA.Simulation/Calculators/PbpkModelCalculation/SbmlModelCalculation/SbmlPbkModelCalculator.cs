using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.ExposureEventsGeneration;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.SbmlModelCalculation {
    public sealed class SbmlPbkModelCalculator : PbkModelCalculatorBase {

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
            ExposureType exposureType,
            IRandom generator
        ) {
            // Get time resolution
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.Resolution);
            var stepsPerDay = getSimulationStepsPerDay();
            var stepLength = 1d / stepsPerDay * timeUnitMultiplier;

            // Initialise exposure events generator
            var exposureEventsGenerator = new ExposureEventsGenerator(
                SimulationSettings,
                KineticModelDefinition.Resolution,
                exposureUnit,
                KineticModelDefinition.Forcings
                    .ToDictionary(
                        r => r.Route,
                        r => r.DoseUnit
                    )
            );

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            var results = new List<PbkSimulationOutput>();
            using (var runner = new SbmlModelRunner(KineticModelInstance, outputMappings)) {

                // Loop over individuals
                foreach (var id in externalIndividualExposures) {

                    // Get simulation period
                    var numberOfDays = getSimulationDuration(id.Individual.Age);
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
                        // The '[..]' bracket notation indicates roadrunner to return
                        // outputs as concentrations
                        var outputTimeSeriesSelection = outputMappings
                            .Select(r => r.TargetUnit.IsPerBodyWeight ? $"[{r.SpeciesId}]" : r.SpeciesId)
                            .ToList();
                        var outputStatesSelection = outputMappings
                            .Select(r => r.CompartmentId)
                            .Distinct()
                            .ToList();
                        simulationOutput = runner.Run(
                            exposureEvents,
                            parameters,
                            outputTimeSeriesSelection,
                            outputStatesSelection,
                            duration,
                            simulationSteps,
                            SimulationSettings.BodyWeightCorrected,
                            KineticModelDefinition.IdBodyWeightParameter
                        );
                    }

                    results.Add(
                        new PbkSimulationOutput() {
                            SubstanceTargetLevelTimeSeries = getSubstanceTargetLevelTimeSeries(
                                outputMappings,
                                id.Individual,
                                simulationOutput,
                                1D / stepsPerDay
                            )
                        }
                    );
                }
            }

            return results;
        }
    }
}
