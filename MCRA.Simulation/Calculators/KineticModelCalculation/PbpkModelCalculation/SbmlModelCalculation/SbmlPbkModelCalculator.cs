using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation {
    public sealed class SbmlPbkModelCalculator : PbkModelCalculatorBase {


        public SbmlPbkModelCalculator(
           KineticModelInstance kineticModelInstance,
           bool useRepeatedDailyEvents
        ) : base(kineticModelInstance, useRepeatedDailyEvents) {
        }

        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            progressState.Update("PBK modelling started");

            // Determine modelled/unmodelled exposure routes
            var modelExposureRoutes = KineticModelInstance.KineticModelDefinition.GetExposureRoutes();

            // Get kinetic model output mappings for selected targets
            var outputMappings = getTargetOutputMappings(targetUnits);

            // Get time resolution
            var stepLength = 1d / KineticModelDefinition.EvaluationFrequency;

            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            using (var runner = new SbmlModelRunner(KineticModelInstance, outputMappings)) {

                // Get exposure event timings (in time-scale of the model)
                var exposureEventTimings = UseRepeatedDailyEvents
                    ? null
                    : getExposureEventTimings(
                        exposureRoutes,
                        _timeUnitMultiplier,
                        _numberOfDays,
                        KineticModelInstance.SpecifyEvents
                    );

                // Loop over individuals
                foreach (var id in externalIndividualExposures.Keys) {
                    var individual = externalIndividualExposures[id].First().Individual;

                    // Create exposure events
                    var exposureEvents = UseRepeatedDailyEvents
                        ? createRepeatedExposureEvent(
                            externalIndividualExposures[id],
                            exposureRoutes,
                            substance,
                            exposureUnit
                        )
                        : createExposureEvents(
                            externalIndividualExposures[id],
                            exposureRoutes,
                            exposureEventTimings,
                            substance,
                            exposureUnit,
                            generator
                        );

                    // Collect individual properties
                    var physiologicalParameters = new Dictionary<string, double>();
                    setPhysiologicalParameterValues(physiologicalParameters, individual);

                    // If we have positive exposures, then run simulation
                    SimulationOutput output = null;
                    if (exposureEvents.Any()) {
                        output = runner.Run(exposureEvents, physiologicalParameters, _evaluationPeriod, _steps + 1);
                    }

                    // Fill results from output
                    var results = new List<SubstanceTargetExposurePattern>();
                    foreach (var outputMapping in outputMappings) {
                        var compartmentWeight = output?.CompartmentVolumes[outputMapping.CompartmentId] ?? double.NaN;
                        var outputTimeSeries = output?.OutputTimeSeries[outputMapping.SpeciesId];
                        var relativeCompartmentWeight = compartmentWeight / individual.BodyWeight;

                        List<TargetExposurePerTimeUnit> exposures = null;
                        if (outputTimeSeries != null && outputTimeSeries.Average() > 0) {
                            if (outputMapping.OutputType == KineticModelOutputType.Concentration) {
                                // Use volume correction if necessary
                                exposures = outputTimeSeries
                                    .Select((r, i) => {
                                        var alignmentFactor = outputMapping.GetUnitAlignmentFactor(compartmentWeight);
                                        var result = new TargetExposurePerTimeUnit(i * stepLength, r * alignmentFactor);
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
                                        var exposure = alignmentFactor * r - runningSum;
                                        runningSum += exposure;
                                        return new TargetExposurePerTimeUnit(i * stepLength, exposure);
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
                            NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                            TimeUnitMultiplier = _timeUnitMultiplier,
                        };
                        results.Add(record);
                    }

                    individualResults[id] = results;
                }
            }

            progressState.Update(100);

            return individualResults;
        }

        private List<IExposureEvent> createRepeatedExposureEvent(
            List<IExternalIndividualDayExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposurePathTypes,
            Compound substance,
            ExposureUnitTriple exposureUnit
        ) {
            var exposureEvents = new List<IExposureEvent>();
            foreach (var exposurePathType in exposurePathTypes) {

                // Get daily doses
                var dailyDoses = externalIndividualExposures
                    .Select(r => r.GetSubstanceExposureForRoute(exposurePathType, substance, true))
                    .ToList();

                // Compute average daily dose
                var averageDailyDose = dailyDoses.Sum() / dailyDoses.Count;

                // Get alignment factor for aligning the substance amount unit of the
                // exposure with the substance amount unit of the PBK model
                var modelInput = KineticModelDefinition.GetInputByPathType(exposurePathType);
                var substanceAmountAlignmentFactor = modelInput.DoseUnit
                    .GetSubstanceAmountUnit()
                    .GetMultiplicationFactor(
                        exposureUnit.SubstanceAmountUnit,
                        substance.MolecularMass
                    );

                // Timescale and exposure unit should be aligned with unit of the model
                var routeExposureEvent = new RepeatingExposureEvent() {
                    Route = modelInput.Route,
                    TimeStart = 0,
                    Value = averageDailyDose / substanceAmountAlignmentFactor,
                    Interval = _timeUnitMultiplier
                };
                exposureEvents.Add(routeExposureEvent);
            }

            return exposureEvents;
        }

        private List<IExposureEvent> createExposureEvents(
            List<IExternalIndividualDayExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposurePathTypes,
            Dictionary<ExposurePathType, List<int>> exposureEventTimings,
            Compound substance,
            ExposureUnitTriple exposureUnit,
            IRandom generator
        ) {
            var exposureEvents = new List<IExposureEvent>();
            foreach (var exposurePathType in exposurePathTypes) {
                // Get daily doses
                var dailyDoses = getRouteSubstanceIndividualDayExposures(
                    externalIndividualExposures,
                    substance,
                    exposurePathType
                );

                // Get alignment factor for aligning the time scale of the
                // exposures with the time scale of the PBK model
                var unitDoses = getUnitDoses(null, dailyDoses, exposurePathType);
                var simulatedDoses = drawSimulatedDoses(
                    unitDoses,
                    exposureEventTimings[exposurePathType].Count,
                    generator
                );

                var modelInput = KineticModelDefinition.GetInputByPathType(exposurePathType);

                // Get alignment factor for aligning the substance amount unit of the
                // exposure with the substance amount unit of the PBK model
                var substanceAmountAlignmentFactor = modelInput.DoseUnit
                    .GetSubstanceAmountUnit()
                    .GetMultiplicationFactor(
                        exposureUnit.SubstanceAmountUnit,
                        substance.MolecularMass
                    );

                // Timescale and exposure unit should be aligned with unit of the model
                var routeExposureEvents = exposureEventTimings[exposurePathType]
                    .Select((r, ix) => new SingleExposureEvent() {
                        Route = modelInput.Route,
                        Time = r,
                        Value = simulatedDoses[ix] / substanceAmountAlignmentFactor
                    })
                    .Where(r => r.Value > 0)
                    .ToList();
                exposureEvents.AddRange(routeExposureEvents);
            }

            return exposureEvents;
        }
    }
}
