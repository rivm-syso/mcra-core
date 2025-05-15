using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation {
    public sealed class SbmlPbkModelCalculator : PbkModelCalculatorBase {

        public SbmlPbkModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            progressState.Update("Starting PBK model simulation");

            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = KineticModelInstance.KineticModelDefinition.GetExposureRoutes();

            // Get time resolution
            var timeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.TimeScale);
            var stepLength = 1d / KineticModelDefinition.EvaluationFrequency;

            // Initialise exposure events generator
            var exposureEventsGenerator = new ExposureEventsGenerator(
                SimulationSettings,
                KineticModelDefinition.TimeScale,
                exposureUnit,
                KineticModelDefinition.Forcings
                    .ToDictionary(
                        r => r.Route,
                        r => r.DoseUnit
                    )
            );

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            using (var runner = new SbmlModelRunner(KineticModelInstance, outputMappings)) {
                // Loop over individuals
                foreach (var id in externalIndividualExposures.Keys) {
                    var individual = externalIndividualExposures[id].First().SimulatedIndividual;

                    // Get simulation period
                    var evaluationPeriod = getEvaluationPeriod(timeUnitMultiplier, individual.Age);
                    var simulationSteps = evaluationPeriod * KineticModelDefinition.EvaluationFrequency + 1;

                    // Create exposure events
                    var exposureEvents = exposureEventsGenerator
                        .CreateExposureEvents(
                            externalIndividualExposures[id],
                            routes,
                            Substance,
                            generator
                        );

                    // Collect individual properties
                    var parameters = new Dictionary<string, double>();
                    setPhysiologicalParameterValues(parameters, individual);

                    // If we have positive exposures, then run simulation
                    SimulationOutput output = null;
                    if (exposureEvents.Any()) {
                        output = runner.Run(
                            exposureEvents,
                            parameters,
                            evaluationPeriod,
                            simulationSteps,
                            SimulationSettings.BodyWeightCorrected,
                            KineticModelDefinition.IdBodyWeightParameter
                        );
                    }

                    // Fill results from output
                    var results = new List<SubstanceTargetExposurePattern>();
                    foreach (var outputMapping in outputMappings) {
                        var compartmentWeight = output?.CompartmentVolumes[outputMapping.CompartmentId] ?? double.NaN;
                        var outputTimeSeries = output?.OutputTimeSeries[outputMapping.SpeciesId];
                        var relativeCompartmentWeight = compartmentWeight / individual.BodyWeight;
                        var outputTimeSeriesBodyWeight = KineticModelDefinition.IdBodyWeightParameter != null
                            ? output?.OutputTimeSeries[KineticModelDefinition.IdBodyWeightParameter]
                            : null;

                        List<TargetExposurePerTimeUnit> exposures = null;
                        if (outputTimeSeries != null && outputTimeSeries.Average() > 0) {
                            if (outputMapping.OutputType == KineticModelOutputType.Concentration) {
                                // Use volume correction if necessary
                                exposures = outputTimeSeries
                                    .Select((r, i) => {
                                        var alignmentFactor = outputMapping.GetUnitAlignmentFactor(compartmentWeight);
                                        var bodyWeight = outputTimeSeriesBodyWeight?.ElementAt(i);
                                        var result = new TargetExposurePerTimeUnit(i * stepLength, r * alignmentFactor, bodyWeight);
                                        return result;
                                    })
                                    .ToList();
                            } else {
                                // The cumulative amounts are reverted to differences between timepoints
                                // (according to the specified resolution, in general hours).
                                var runningSum = 0D;
                                exposures = outputTimeSeries
                                    .Select((r, i) => {
                                        var alignmentFactor = outputMapping.GetUnitAlignmentFactor(compartmentWeight);
                                        var bodyWeight = outputTimeSeriesBodyWeight?.ElementAt(i);
                                        var exposure = alignmentFactor * r - runningSum;
                                        runningSum += exposure;
                                        return new TargetExposurePerTimeUnit(i * stepLength, exposure, bodyWeight);
                                    })
                                    .ToList();
                            }
                        }

                        var record = new SubstanceTargetExposurePattern() {
                            Substance = outputMapping.Substance,
                            Target = outputMapping.TargetUnit.Target,
                            Compartment = outputMapping.CompartmentId,
                            RelativeCompartmentWeight = outputTimeSeries != null
                                ? relativeCompartmentWeight : double.NaN,
                            ExposureType = exposureType,
                            TargetExposuresPerTimeUnit = exposures ?? [],
                            NonStationaryPeriod = SimulationSettings.NonStationaryPeriod,
                            TimeUnitMultiplier = timeUnitMultiplier,
                        };
                        results.Add(record);
                    }

                    individualResults[id] = results;
                }
            }

            progressState.Update("PBK model simulation finished", 100);
            return individualResults;
        }
    }
}
