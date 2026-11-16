using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.TargetExposureFromTimeSeriesCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public class KineticConversionFactorModelCalculator {

        public List<KineticConversionFactorCalculationResult> ComputeFromPbk(
            ICollection<Compound> activeSubstance,
            ICollection<KineticModelInstance> pbkModelInstances,
            PbkSimulationSettings simulationSettings,
            ICollection<SimpleAbsorptionFactor> absorptionFactors,
            ExposureUnitTriple externalExposureUnit,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            ICollection<ExposureRoute> exposureRoutes,
            ICollection<SimulatedIndividual> individuals,
            double exposureMin,
            double exposureMax,
            int numSimulations,
            bool computeInternalTargetConversionFactors,
            bool keepPbkSimulationOutputs,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorCalculationResult>();

            var exposures = GriddingFunctions.LogSpace(exposureMin, exposureMax, numSimulations).ToList();
            var pbkModelLookup = pbkModelInstances
                .ToDictionary(r => r.InputSubstance);

            var timeSeriesExposureCharacterisationCalculator = TimeSeriesExposureCharacterisationCalculatorFactory
                .Create(exposureType, simulationSettings);

            var absorptionFactorsLookup = absorptionFactors?.ToDictionary(r => (r.ExposureRoute, r.Substance)) ?? [];
            foreach (var substance in activeSubstance) {
                if (!pbkModelLookup.TryGetValue(substance, out var pbkModelInstance)) {
                    throw new Exception($"No PBK model for substance {substance.Name} ({substance.Code}).");
                }
                var substanceAbsorptionFactors = exposureRoutes.ToDictionary(
                    r => r,
                    r => absorptionFactorsLookup.TryGetValue((r, substance), out var valSpecific)
                        ? valSpecific.AbsorptionFactor
                        : absorptionFactorsLookup.TryGetValue((r, null), out var valGeneric) ? valGeneric.AbsorptionFactor : 1D
                );
                for (int i = 0; i < exposureRoutes.Count; i++) {
                    var route = exposureRoutes.ElementAt(i);
                    var record = ComputeFromPbk(
                        substance,
                        pbkModelInstance,
                        simulationSettings,
                        substanceAbsorptionFactors,
                        externalExposureUnit,
                        targetUnits,
                        timeSeriesExposureCharacterisationCalculator,
                        route,
                        exposures,
                        individuals,
                        i == 0 && computeInternalTargetConversionFactors,
                        keepPbkSimulationOutputs,
                        progressState,
                        generator
                    );
                    result.Add(record);
                }
            }
            return result;
        }

        public static KineticConversionFactorCalculationResult ComputeFromPbk(
            Compound substance,
            KineticModelInstance pbkModelInstance,
            PbkSimulationSettings simulationSettings,
            Dictionary<ExposureRoute, double> absorptionFactors,
            ExposureUnitTriple externalExposureUnit,
            ICollection<TargetUnit> targetUnits,
            ITimeSeriesExposureCharacterisationCalculator timeSeriesExposureCharacterisationCalculator,
            ExposureRoute route,
            List<double> doses,
            ICollection<SimulatedIndividual> individuals,
            bool computeInternalTargetConversionFactors,
            bool keepPbkSimulationOutputs,
            ProgressState progressState,
            IRandom generator
        ) {
            var conversionFactorRecords = new List<IKineticConversionFactorModel>();

            // If no individuals were provided, create one nominal individual
            if (individuals == null || individuals.Count == 0) {
                var individual = new SimulatedIndividual(new Individual(0), 0);
                if (pbkModelInstance.TryGetParameterValue(PbkModelParameterType.BodyWeight, out var bw)) {
                    individual.BodyWeight = bw;
                } else if (pbkModelInstance.TryGetParameterValue(PbkModelParameterType.BodyWeightRef, out var bwRef)) {
                    individual.BodyWeight = bwRef;
                    if (pbkModelInstance.TryGetParameterValue(PbkModelParameterType.AgeRef, out var ageRef)) {
                        individual.Age = ageRef;
                    }
                }
                individuals = [individual];
            }

            // Create PBK model calculator
            var pbkModelCalculator = PbkModelCalculatorFactory
                .Create(pbkModelInstance, simulationSettings, absorptionFactors);

            // Create external individual exposures
            var externalTargetUnit = new TargetUnit(new ExposureTarget(route), externalExposureUnit);
            var externalIndividualExposures = createExternalIndividualExposures(
                individuals,
                substance,
                route,
                doses,
                simulationSettings.ApplyBodyWeightScaling
            );
            var simulatedDoses = externalIndividualExposures.Select(r => r.dose).ToList();
            var simulatedExposures = externalIndividualExposures.Select(r => (r.Item1, r.Item3)).ToList();

            // Run PBK model simulations
            var simulationResults = pbkModelCalculator.Calculate(
                simulatedExposures,
                externalExposureUnit,
                [route],
                targetUnits,
                generator,
                progressState
            );

            // Compute external->internal KCFs
            var simulationTargetExposures = simulationResults
                .Select(timeSeriesExposureCharacterisationCalculator.ComputeSubstanceTargetExposures)
                .ToList();
            foreach (var targetUnit in targetUnits) {
                var targetExposures = simulationTargetExposures
                    .Select(r => r[targetUnit.Target])
                    .ToList();
                foreach (var toSubstance in pbkModelCalculator.OutputSubstances) {
                    var toTargetExposures = targetExposures
                        .Select(r => r.TryGetValue(toSubstance, out var val) ? val : null)
                        .Select(r => r?.Exposure ?? 0)
                        .ToList();
                    var record = createConversionModel(
                        substance,
                        toSubstance,
                        externalTargetUnit,
                        simulatedDoses,
                        targetUnit,
                        toTargetExposures
                    );
                    conversionFactorRecords.Add(record);
                }
            }

            // Compute internal<->internal KCFs
            if (computeInternalTargetConversionFactors) {
                foreach (var fromTargetUnit in targetUnits) {
                    foreach (var toTargetUnit in targetUnits) {
                        if (fromTargetUnit != toTargetUnit) {
                            var fromTargetExposures = simulationTargetExposures
                                .Select(r => r[fromTargetUnit.Target])
                                .Select(r => r.TryGetValue(substance, out var val) ? val : null)
                                .Select(r => r?.Exposure ?? 0)
                                .ToList();
                            foreach (var toSubstance in pbkModelCalculator.OutputSubstances) {
                                var toTargetExposures = simulationTargetExposures
                                    .Select(r => r[toTargetUnit.Target])
                                    .Select(r => r.TryGetValue(toSubstance, out var val) ? val : null)
                                    .Select(r => r?.Exposure ?? 0)
                                    .ToList();
                                var record = createConversionModel(
                                    substance,
                                    toSubstance,
                                    fromTargetUnit,
                                    fromTargetExposures,
                                    toTargetUnit,
                                    toTargetExposures
                                );
                                conversionFactorRecords.Add(record);
                            }
                        }
                    }
                }
            }

            var result = new KineticConversionFactorCalculationResult() {
                Route = route,
                Substance = substance,
                ExposureUnit = externalExposureUnit,
                ConversionFactors = conversionFactorRecords,
                PbkSimulationOutputs = keepPbkSimulationOutputs
                    ? simulationResults.Zip(simulatedDoses, (r, d) => (d, r)).ToList()
                    : null,
            };
            return result;
        }

        private static List<(SimulatedIndividual, double dose, List<IExternalIndividualDayExposure>)> createExternalIndividualExposures(
            ICollection<SimulatedIndividual> individuals,
            Compound substance,
            ExposureRoute route,
            List<double> doses,
            bool bwAdjusted
        ) {
            if (bwAdjusted && individuals.Any(r => double.IsNaN(r.BodyWeight))) {
                throw new NotImplementedException("Cannot run PBK model with bodyweight adjusted exposures for individuals with a missing bodyweight.");
            }

            var result = new List<(SimulatedIndividual individual, double dose, List<IExternalIndividualDayExposure> exposure)>();
            var ix = 0;
            foreach (var individual in individuals) {
                foreach (var dose in doses) {
                    var pathExposures = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                        [new ExposurePath(ExposureSource.Undefined, route)] = [
                            new AggregateIntakePerCompound() {
                                // If BW adjusted, then multiply the amount by BW to get per BW exposure
                                Amount = bwAdjusted ? dose * individual.BodyWeight : dose,
                                Compound = substance,
                            }
                        ]
                    };
                    var individualDayExposure = new ExternalIndividualDayExposure(pathExposures) {
                        SimulatedIndividual = individual,
                        SimulatedIndividualDayId = ix++,
                    };
                    var individualExposure = new ExternalIndividualExposure(individual, pathExposures) {
                        ExternalIndividualDayExposures = [individualDayExposure],
                    };
                    result.Add((
                        individualExposure.SimulatedIndividual,
                        dose,
                        individualExposure.ExternalIndividualDayExposures
                    ));
                }
            }
            return result;
        }

        private static KineticConversionFactorEmpiricalModel createConversionModel(
            Compound fromSubstance,
            Compound toSubstance,
            TargetUnit fromTargetUnit,
            List<double> fromTargetExposures,
            TargetUnit toTargetUnit,
            List<double> toTargetExposures
        ) {
            var conversionRecords = toTargetExposures
                .Zip(fromTargetExposures, (t, e) => new KineticConversionDataRecord(e, t))
                .ToList();
            var lmFit = SimpleLinearRegressionCalculator.Compute(
                [.. conversionRecords.Select(r => r.FromExposure)],
                [.. conversionRecords.Select(r => r.ToExposure)],
                Intercept.Omit
            );
            var record = new KineticConversionFactorEmpiricalModel() {
                SubstanceFrom = fromSubstance,
                TargetFrom = fromTargetUnit.Target,
                UnitFrom = fromTargetUnit.ExposureUnit,
                SubstanceTo = toSubstance,
                TargetTo = toTargetUnit.Target,
                UnitTo = toTargetUnit.ExposureUnit,
                KineticConversionDataRecords = conversionRecords,
                Factor = lmFit.Coefficient
            };
            return record;
        }
    }
}
