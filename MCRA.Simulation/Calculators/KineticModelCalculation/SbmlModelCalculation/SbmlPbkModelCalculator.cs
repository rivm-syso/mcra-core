using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation {
    public sealed class SbmlPbkModelCalculator : PbkModelCalculatorBase {

        public SbmlPbkModelCalculator(
           KineticModelInstance kineticModelInstance,
           IDictionary<ExposurePathType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance, defaultAbsorptionFactors) {
        }

        public static string GetModelFilePath(string fileName) {
            var location = typeof(SbmlPbkModelCalculator).Assembly.Location;
            var assemblyFolder = new FileInfo(location).Directory.FullName;
            var pathSbmlfile = Path.Combine(assemblyFolder, "Resources", "KineticModels", fileName);
            return pathSbmlfile;
        }

        protected override Dictionary<int, List<SubstanceTargetExposurePattern>> calculate(
            IDictionary<int, List<IExternalIndividualDayExposure>> externalIndividualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double relativeCompartmentWeightObsolete,
            bool isNominal,
            IRandom generator,
            ProgressState progressState
        ) {
            progressState.Update("PBPK modelling started");
            
            var unforcedExposureRoutes = exposureRoutes.Except(_modelInputDefinitions.Keys).ToList();

            // Get output parameter and output unit
            // TODO: allow more than one substance output mapping
            var selectedOutputs = _outputSubstancesMappings.Take(1).First();
            var outputSubstanceMappings = _outputSubstancesMappings.Take(1).ToList();

            var isOutputConcentration = _selectedModelOutputDefinition.TargetUnit.IsPerBodyWeight();
            
            var substanceAmountUnit = exposureUnit.SubstanceAmountUnit;

            var individualResults = new Dictionary<int, List<SubstanceTargetExposurePattern>>();
            using (var runner = new SbmlModelRunner(KineticModelInstance)) {

                // Get exposure event timings (in time-scale of the model)
                var exposureEventTimings = getExposureEventTimings(
                    exposureRoutes,
                    (int)_timeUnitMultiplier,
                    _numberOfDays,
                    KineticModelInstance.SpecifyEvents
                );

                // Loop over individuals
                foreach (var id in externalIndividualExposures.Keys) {
                    var individual = externalIndividualExposures[id].First().Individual;

                    // Create exposure events
                    var exposureEvents = createExposureEvents(
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

                    // Target exposures for unsupported routes
                    var targetExposureUnForced = calculateUnforcedRoutesSubstanceAmount(
                        externalIndividualExposures[id],
                        substance,
                        unforcedExposureRoutes,
                        double.NaN
                    );
                    // TODO
                    targetExposureUnForced = 0;

                    // If we have positive exposures, then run simulation
                    var output = (exposureEvents.Any(r => r.Events.Any()))
                        ? runner.Run(exposureEvents, physiologicalParameters, _evaluationPeriod, _steps + 1)
                        : null;


                    // Fill results from output
                    var result = new List<SubstanceTargetExposurePattern>();
                    foreach (var outputSubstanceMapping in outputSubstanceMappings) {
                        //TODO check
                        var compartmentWeight = runner.GetCompartmentVolume(outputSubstanceMapping.Value.outputDef.Id);
                        var outputTimeSeries = output?.OutputTimeSeries[outputSubstanceMapping.Key];
                        var relativeCompartmentWeight = compartmentWeight/individual.BodyWeight;
                        var reverseIntakeUnitConversionFactor = _selectedModelOutputDefinition
                            .TargetUnit.ExposureUnit
                            .GetAlignmentFactor(
                                exposureUnit,
                                substance.MolecularMass,
                                relativeCompartmentWeight
                            );
                        List<TargetExposurePerTimeUnit> exposures;

                        if (outputTimeSeries != null && outputTimeSeries.Average() > 0) {
                            if (_modelOutputs[outputSubstanceMapping.Key].Type == KineticModelOutputType.Concentration) {
                                // Use volume correction if necessary
                                var correctionVolume = isOutputConcentration
                                    ? compartmentWeight : 1D;
                                exposures = outputTimeSeries
                                    .Select((r, i) => new TargetExposurePerTimeUnit(i, r * reverseIntakeUnitConversionFactor * correctionVolume))
                                    .ToList();
                            } else {
                                // The cumulative amounts are reverted to differences between timepoints
                                // (according to the specified resolution, in general hours).
                                var runningSum = 0D;
                                exposures = outputTimeSeries
                                    .Select((r, i) => {
                                        var exposure = r * reverseIntakeUnitConversionFactor - runningSum;
                                        runningSum += exposure;
                                        return new TargetExposurePerTimeUnit(i, exposure);
                                    })
                                    .ToList();
                            }
                        } else {
                            exposures = new() { new() };
                        }

                        var record = new SubstanceTargetExposurePattern() {
                            Substance = outputSubstanceMapping.Value.substance,
                            Compartment = outputSubstanceMapping.Value.outputDef.Id,
                            RelativeCompartmentWeight = relativeCompartmentWeight,
                            ExposureType = exposureType,
                            TargetExposuresPerTimeUnit = exposures,
                            NonStationaryPeriod = KineticModelInstance.NonStationaryPeriod,
                            TimeUnitMultiplier = (int)_timeUnitMultiplier * KineticModelDefinition.EvaluationFrequency,
                            OtherRouteExposures = targetExposureUnForced
                        };
                        result.Add(record);
                    }

                    individualResults[id] = result;
                }
            }

            progressState.Update(100);

            return individualResults;
        }

        private List<SimulationInput> createExposureEvents(
            List<IExternalIndividualDayExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposurePathTypes,
            Dictionary<ExposurePathType, List<int>> exposureEventTimings,
            Compound substance,
            ExposureUnitTriple exposureUnit,
            IRandom generator
        ) {
            var exposureEvents = new List<SimulationInput>();
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
                var routeExposureEvents = new SimulationInput() {
                    Route = modelInput.Route,
                    Events = exposureEventTimings[exposurePathType]
                        .Select((r, ix) => (Time: (double)r, Value: simulatedDoses[ix] / substanceAmountAlignmentFactor))
                        .Where(r => r.Value > 0)
                        .ToList()
                };
                exposureEvents.Add(routeExposureEvents);
            }

            return exposureEvents;
        }
    }
}
