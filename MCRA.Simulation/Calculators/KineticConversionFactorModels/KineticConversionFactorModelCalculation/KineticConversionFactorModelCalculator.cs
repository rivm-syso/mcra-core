using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels.KineticConversionFactorModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {
    public class KineticConversionFactorModelCalculator {

        public static List<IKineticConversionFactorModel> ComputeFromPbk(
            ICollection<Compound> activeSubstance,
            ICollection<KineticModelInstance> pbkModelInstances,
            PbkSimulationSettings simulationSettings,
            ExposureUnitTriple externalExposureUnit,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            ICollection<ExposureRoute> exposureRoutes,
            double exposureMin,
            double exposureMax,
            int numSimulations,
            bool computeInternalTargetConversionFactors,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<IKineticConversionFactorModel>();
            var exposures = GriddingFunctions.LogSpace(exposureMin, exposureMax, numSimulations).ToList();
            var pbkModelLookup = pbkModelInstances
                .ToDictionary(r => r.InputSubstance);
            foreach (var substance in activeSubstance) {
                if (!pbkModelLookup.TryGetValue(substance, out var pbkModelInstance)) {
                    throw new Exception($"No PBK model for substnace {substance.Name} ({substance.Code}).");
                }
                for (int i = 0; i < exposureRoutes.Count; i++) {
                    var route = exposureRoutes.ElementAt(i);
                    var records = ComputeFromPbk(
                        substance,
                        pbkModelInstance,
                        simulationSettings,
                        externalExposureUnit,
                        targetUnits,
                        exposureType,
                        route,
                        exposures,
                        i == 0 && computeInternalTargetConversionFactors,
                        progressState,
                        generator
                    );
                    result.AddRange(records);
                }
            }
            return result;
        }

        public static List<IKineticConversionFactorModel> ComputeFromPbk(
            Compound substance,
            KineticModelInstance pbkModelInstance,
            PbkSimulationSettings simulationSettings,
            ExposureUnitTriple externalExposureUnit,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            ExposureRoute route,
            List<double> doses,
            bool computeInternalTargetConversionFactors,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<IKineticConversionFactorModel>();
            var pbkModelCalculator = PbkModelCalculatorFactory
                .Create(pbkModelInstance, simulationSettings);

            var simulatedIndividualExposures = new List<IExternalIndividualExposure>();
            var ix = 0;
            foreach (var dose in doses) {
                var individual = new Individual(ix++);
                var simulatedIndividual = new SimulatedIndividual(individual, ix);
                var pathExposures = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                    [new ExposurePath(ExposureSource.Undefined, route)] = [
                        new AggregateIntakePerCompound() {
                            Amount = dose,
                            Compound = substance,
                        }
                    ]
                };
                var individualDayExposure = new ExternalIndividualDayExposure(pathExposures) {
                    SimulatedIndividual = simulatedIndividual,
                    SimulatedIndividualDayId = 0,
                };
                var individualExposure = new ExternalIndividualExposure(simulatedIndividual, pathExposures) {
                    ExternalIndividualDayExposures = [individualDayExposure],
                };
                simulatedIndividualExposures.Add(individualExposure);
            }

            var pbkKineticConversionCalculator = new PbkKineticConversionCalculator(pbkModelCalculator);
            var simulationResults = pbkKineticConversionCalculator
                .CalculateIndividualTargetExposures(
                    simulatedIndividualExposures,
                    [route],
                    externalExposureUnit,
                    targetUnits,
                    progressState,
                    generator
                );

            var externalTargetUnit = new TargetUnit(new ExposureTarget(route), externalExposureUnit);
            foreach (var targetUnit in targetUnits) {
                var targetExposures = simulationResults
                    .Select(r => r.InternalTargetExposures[targetUnit.Target])
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
                        doses,
                        targetUnit,
                        toTargetExposures
                    );
                    result.Add(record);
                }
            }

            if (computeInternalTargetConversionFactors) {
                foreach (var fromTargetUnit in targetUnits) {
                    foreach (var toTargetUnit in targetUnits) {
                        if (fromTargetUnit != toTargetUnit) {
                            var fromTargetExposures = simulationResults
                                .Select(r => r.InternalTargetExposures[fromTargetUnit.Target])
                                .Select(r => r.TryGetValue(substance, out var val) ? val : null)
                                .Select(r => r?.Exposure ?? 0)
                                .ToList();
                            foreach (var toSubstance in pbkModelCalculator.OutputSubstances) {
                                var toTargetExposures = simulationResults
                                    .Select(r => r.InternalTargetExposures[toTargetUnit.Target])
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
                                result.Add(record);
                            }
                        }
                    }
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
                conversionRecords.Select(r => r.FromExposure).ToList(),
                conversionRecords.Select(r => r.ToExposure).ToList(),
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
