﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.PbkModelParameterDistributionModels;
using MCRA.Simulation.Calculators.PbpkModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation {
    public abstract class PbkModelCalculatorBase : IPbkModelCalculator {

        // Model instance
        public KineticModelInstance KineticModelInstance { get; }
        public Compound Substance => KineticModelInstance.InputSubstance;
        public List<Compound> OutputSubstances => KineticModelInstance.Substances;

        // Model definition
        public KineticModelDefinition KineticModelDefinition => KineticModelInstance.KineticModelDefinition;
        protected IDictionary<ExposureRoute, KineticModelInputDefinition> _modelInputDefinitions;
        protected IDictionary<string, KineticModelParameterDefinition> _modelParameterDefinitions;

        // Run/simulation settings
        public PbkSimulationSettings SimulationSettings { get; }

        // Matrices that can be used for extrapolation when blood is requested
        public static readonly Dictionary<BiologicalMatrix, HashSet<BiologicalMatrix>> ExtrapolationMatrices = 
            new () {
            { BiologicalMatrix.Blood, [
                BiologicalMatrix.Blood,
                BiologicalMatrix.VenousBlood,
                BiologicalMatrix.ArterialBlood ]
            },
            { BiologicalMatrix.BloodPlasma, [
                BiologicalMatrix.BloodPlasma,
                BiologicalMatrix.VenousPlasma,
                BiologicalMatrix.ArterialPlasma ]
            }
        };

        public PbkModelCalculatorBase(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) {
            KineticModelInstance = kineticModelInstance;
            SimulationSettings = simulationSettings;

            // Lookups/dictionaries for model definition elements
            _modelParameterDefinitions = KineticModelDefinition.Parameters?
                .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);

            // Check if model matches settings
            if (SimulationSettings.PbkSimulationMethod != PbkSimulationMethod.Standard
                && SimulationSettings.BodyWeightCorrected
                && string.IsNullOrEmpty(KineticModelDefinition.IdBodyWeightParameter)
            ) {
                var msg = $"Cannot apply bodyweight corrected exposures on PBK model [{KineticModelDefinition.Id}]: no BW parameter found.";
                throw new Exception(msg);
            }
        }

        public List<PbkSimulationOutput> Calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple externalExposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            IRandom generator,
            ProgressState progressState
        ) {
            // Check if routes are supported by the model
            var missingRoutes = routes.Except(KineticModelDefinition.GetExposureRoutes()).ToList();
            if (missingRoutes.Count > 0) {
                var routeNames = string.Join(", ", missingRoutes.Select(r => r.GetDisplayName()));
                throw new Exception($"Exposure routes {routeNames} are not supported by PBK model {KineticModelDefinition.Id}.");
            }

            progressState?.Update("Starting PBK model simulation");
            var result = calculate(
                externalIndividualExposures,
                externalExposureUnit,
                routes,
                targetUnits,
                exposureType,
                generator
            );

            progressState?.Update("PBK model simulation finished", 100);
            return result;
        }

        /// <summary>
        /// Runs the PBK model for the provided external individual exposures.
        /// </summary>
        public abstract List<PbkSimulationOutput> calculate(
            ICollection<(SimulatedIndividual Individual, List<IExternalIndividualDayExposure> IndividualDayExposures)> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            ICollection<ExposureRoute> routes,
            ICollection<TargetUnit> targetUnits,
            ExposureType exposureType,
            IRandom generator
        );

        /// <summary>
        /// Returns a draw of the parameter values or nominal values
        /// </summary>
        protected virtual Dictionary<string, double> drawParameters(
            IDictionary<string, KineticModelInstanceParameter> parameters,
            IRandom random,
            bool useParameterVariability
        ) {
            var drawn = new Dictionary<string, double>();
            if (!useParameterVariability) {
                drawn = parameters.ToDictionary(c => c.Key, c => c.Value.Value);
            } else {
                foreach (var parameter in parameters) {
                    var model = PbkModelParameterDistributionModelFactory.Create(parameter.Value.DistributionType);
                    model.Initialize(parameter.Value.Value, parameter.Value.CvVariability);
                    drawn.Add(parameter.Key, model.Sample(random));
                }
            }
            return drawn;
        }

        protected void setPhysiologicalParameterValues(
            Dictionary<string, double> parametrisation,
            SimulatedIndividual individual
        ) {
            var instanceParameters = KineticModelInstance.KineticModelInstanceParameters;
            var bodyWeightParameter = KineticModelDefinition.Parameters
                .FirstOrDefault(r => r.Id == KineticModelDefinition.IdBodyWeightParameter);

            // Set BW
            //IsInternalParameter is equal to IsConstant, see also SbmlToPbkModelDefinitionConverter where IsInternalParameter is set
            //Meaning of IsConstant: true = parameter is set from outside
            //                       false = parameter is calculated in PBK model
            if (!bodyWeightParameter.IsInternalParameter) {
                // TODO: current code assumes bodyweights in same unit as kinetic model parameter
                var bodyWeight = individual.BodyWeight;
                parametrisation[KineticModelDefinition.IdBodyWeightParameter] = bodyWeight;

                // Set BSA
                var bodySurfaceAreaParameter = KineticModelDefinition.Parameters
                    .FirstOrDefault(r => r.Id == KineticModelDefinition.IdBodySurfaceAreaParameter);

                if (!bodySurfaceAreaParameter?.IsInternalParameter ?? false) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdBodySurfaceAreaParameter, out var bsaParameterValue)
                        && instanceParameters.TryGetValue(KineticModelDefinition.IdBodyWeightParameter, out var bwParameterValue)
                    ) {
                        var standardBSA = bsaParameterValue.Value;
                        var standardBW = bwParameterValue.Value;
                        var allometricScaling = Math.Pow(standardBW / bodyWeight, 1 - 0.7);
                        parametrisation[KineticModelDefinition.IdBodySurfaceAreaParameter] = standardBSA / allometricScaling;
                    }
                }
            }

            // Set age
            var ageParameter = KineticModelDefinition.Parameters
                .FirstOrDefault(r => r.Id == KineticModelDefinition.IdAgeParameter);
            if (!ageParameter?.IsInternalParameter ?? false) {
                // Get individual age
                // TODO: current code assumes age in same unit as kinetic model parameter
                var age = individual.Age;
                if (!age.HasValue || double.IsNaN(age.Value)) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdAgeParameter, out var ageParameterValue)
                        && !double.IsNaN(ageParameterValue.Value)
                    ) {
                        // Fallback on age from kinetic model parametrisation
                        age = ageParameterValue.Value;
                    } else if (_modelParameterDefinitions[KineticModelDefinition.IdAgeParameter].DefaultValue.HasValue) {
                        // Fallback on default age from kinetic model definition
                        age = _modelParameterDefinitions[KineticModelDefinition.IdAgeParameter].DefaultValue.Value;
                    } else {
                        throw new Exception($"Cannot set required parameter for age for PBK model [{KineticModelDefinition.Name}].");
                    }
                }
                parametrisation[KineticModelDefinition.IdAgeParameter] = age.Value;
            }

            // Set sex
            var sexParameter = KineticModelDefinition.Parameters
                .FirstOrDefault(r => r.Id == KineticModelDefinition.IdAgeParameter);
            if (!sexParameter?.IsInternalParameter ?? false) {
                // TODO: implicit assumption of Female = 1, Male = 2 should become explicit
                var sex = individual.Gender;
                if (sex == GenderType.Undefined) {
                    if (instanceParameters.TryGetValue(KineticModelDefinition.IdSexParameter, out var paramValue)
                        && !double.IsNaN(paramValue.Value)
                    ) {
                        // Fallback on age from kinetic model parametrisation
                        sex = (GenderType)paramValue.Value;
                    } else if (_modelParameterDefinitions[KineticModelDefinition.IdSexParameter].DefaultValue.HasValue) {
                        // Fallback on default age from kinetic model definition
                        sex = (GenderType)_modelParameterDefinitions[KineticModelDefinition.IdSexParameter].DefaultValue;
                    } else {
                        throw new Exception($"Cannot set required parameter for sex for PBK model [{KineticModelDefinition.Name}].");
                    }
                }
                parametrisation[KineticModelDefinition.IdSexParameter] = (double)sex;
            }
        }

        /// <summary>
        /// Gets the duration of the simulation in days.
        /// </summary>
        protected double getSimulationDuration(
            double? currentAge
        ) {
            switch (SimulationSettings.PbkSimulationMethod) {
                case PbkSimulationMethod.Standard:
                    return SimulationSettings.NumberOfSimulatedDays;
                case PbkSimulationMethod.LifetimeToSpecifiedAge:
                    return (int)(SimulationSettings.LifetimeYears * 365.25);
                case PbkSimulationMethod.LifetimeToCurrentAge:
                    if (!currentAge.HasValue) {
                        throw new Exception("Cannot run PBK model simulation to current age for individuals with undefined age.");
                    }
                    return currentAge.Value * 365.25;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the number of evaluations per day.
        /// </summary>
        protected double getSimulationStepsPerDay() {
            if (SimulationSettings.PbkSimulationMethod == PbkSimulationMethod.LifetimeToSpecifiedAge
                || SimulationSettings.PbkSimulationMethod == PbkSimulationMethod.LifetimeToCurrentAge
            ) {
                // For lifetime models, use one evaluation per day
                return 1;
            }
            if (SimulationSettings.OutputResolutionTimeUnit == PbkModelOutputResolutionTimeUnit.ModelTimeUnit) {
                // Use model resolution and frequency
                var modelTimeUnitMultiplier = TimeUnit.Days.GetTimeUnitMultiplier(KineticModelDefinition.Resolution);
                return modelTimeUnitMultiplier * KineticModelDefinition.EvaluationFrequency;
            } else {
                // Compute number of evaluations per day
                if (SimulationSettings.OutputResolutionTimeUnit == PbkModelOutputResolutionTimeUnit.Minutes) {
                    return 24D * 60 / SimulationSettings.OutputResolutionStepSize;
                } else if (SimulationSettings.OutputResolutionTimeUnit == PbkModelOutputResolutionTimeUnit.Hours) {
                    return 24D / SimulationSettings.OutputResolutionStepSize;
                } else if (SimulationSettings.OutputResolutionTimeUnit == PbkModelOutputResolutionTimeUnit.Days) {
                    return 1D / SimulationSettings.OutputResolutionStepSize;
                } else {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Gets the output mappings for the specified target units.
        /// </summary>
        /// <param name="targetUnits"></param>
        /// <returns></returns>
        protected List<TargetOutputMapping> getTargetOutputMappings(
            ICollection<TargetUnit> targetUnits
        ) {
            var result = new List<TargetOutputMapping>();
            foreach (var targetUnit in targetUnits) {
                var output = KineticModelDefinition.Outputs
                    .FirstOrDefault(c => c.TargetUnit.Target == targetUnit.Target);

                if (output == null && targetUnit.TargetLevelType == TargetLevelType.Internal) {
                    // Try to find an alternative output in case the biological matrix does not match.
                    // E.g., when target is blood, but model only has venous/arterial blood, use venous blood.
                    if (ExtrapolationMatrices.TryGetValue(targetUnit.BiologicalMatrix, out var extrapolationMatrices)) {
                        foreach (var matrix in extrapolationMatrices) {
                            output = KineticModelDefinition.Outputs
                                .FirstOrDefault(c => c.TargetUnit.Target.TargetLevelType == TargetLevelType.Internal
                                    && c.TargetUnit.Target.BiologicalMatrix == matrix);
                            if (output != null) {
                                break;
                            }
                        }
                    }
                }

                if (output == null) {
                    var msg = $"No output found in PBK model [{KineticModelDefinition.Id}] for target [{targetUnit.Target.GetDisplayName()}].";
                    throw new Exception(msg);
                }
                var codeCompartment = output.Id;
                if (output.Species?.Count > 0) {
                    foreach (var species in output.Species) {
                        var substance = !string.IsNullOrEmpty(species.IdSubstance)
                            ? KineticModelInstance.KineticModelSubstances
                                .FirstOrDefault(r => r.SubstanceDefinition?.Id == species.IdSubstance)?.Substance
                            : KineticModelInstance.Substances.FirstOrDefault();
                        if (substance != null) {
                            var record = new TargetOutputMapping() {
                                CompartmentId = codeCompartment,
                                SpeciesId = species.IdSpecies,
                                Substance = substance,
                                OutputDefinition = output,
                                TargetUnit = targetUnit
                            };
                            result.Add(record);
                        } else {
                            // TODO: what to do when output substance is not defined?
                            // It seems reasonable to allow this when the missing substance is a
                            // metabolite and to throw an exception when the missing substance is
                            // the parent/input substance.
                        }
                    }
                } else {
                    var record = new TargetOutputMapping() {
                        CompartmentId = codeCompartment,
                        SpeciesId = codeCompartment,
                        Substance = KineticModelInstance.Substances.Single(),
                        OutputDefinition = output,
                        TargetUnit = targetUnit
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        protected static List<SubstanceTargetExposureTimeSeries> getSubstanceTargetLevelTimeSeries(
            List<TargetOutputMapping> outputMappings,
            SimulatedIndividual individual,
            SimulationOutput simulationOutput,
            double stepLength
        ) {
            // Store results of selected compartments/species
            var resultTimeSeries = new List<SubstanceTargetExposureTimeSeries>();
            foreach (var outputMapping in outputMappings) {
                var compartmentSize = simulationOutput?.OutputStates[outputMapping.CompartmentId] ?? double.NaN;
                var outputTimeSeries = simulationOutput?.OutputTimeSeries[outputMapping.SpeciesId];
                var relativeCompartmentWeight = compartmentSize / individual.BodyWeight;

                List<SubstanceTargetExposureTimePoint> exposures = null;
                if (outputTimeSeries != null && outputTimeSeries.Any(r => r > 0)) {
                    if (outputMapping.OutputType == KineticModelOutputType.Concentration) {
                        exposures = outputTimeSeries
                            .Select((r, i) => {
                                return new SubstanceTargetExposureTimePoint(
                                    i * stepLength,
                                    outputMapping.GetUnitAlignmentFactor(compartmentSize) * r
                                );
                            })
                            .ToList(); ;
                    } else if (outputMapping.OutputType == KineticModelOutputType.CumulativeAmount) {
                        // The cumulative amounts are reverted to differences between timepoints
                        // (according to the specified resolution, in general hours).
                        var runningSum = 0D;
                        exposures = outputTimeSeries
                            .Select((r, i) => {
                                var alignmentFactor = outputMapping.GetUnitAlignmentFactor(compartmentSize);
                                var exposure = alignmentFactor * r - runningSum;
                                runningSum += exposure;
                                return new SubstanceTargetExposureTimePoint(
                                    i * stepLength,
                                    exposure
                                );
                            })
                            .ToList();
                    } else {
                        throw new NotImplementedException();
                    }
                }

                resultTimeSeries.Add(
                    new SubstanceTargetExposureTimeSeries() {
                        Substance = outputMapping.Substance,
                        TargetUnit = outputMapping.TargetUnit,
                        RelativeCompartmentWeight = relativeCompartmentWeight,
                        Exposures = exposures
                    }
                );
            }

            return resultTimeSeries;
        }
    }
}
