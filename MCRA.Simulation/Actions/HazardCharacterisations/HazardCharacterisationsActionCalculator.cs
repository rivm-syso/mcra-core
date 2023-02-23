using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.AggregateHazardCharacterisationCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationImputation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    [ActionType(ActionType.HazardCharacterisations)]
    public sealed class HazardCharacterisationsActionCalculator : ActionCalculatorBase<HazardCharacterisationsActionResult> {

        public HazardCharacterisationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var multipleSubstances = _project.AssessmentSettings.MultipleSubstances;
            var restrictToAvailableHazardCharacterisations = _project.EffectSettings.RestrictToAvailableHazardCharacterisations;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = multipleSubstances && !restrictToAvailableHazardCharacterisations;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = multipleSubstances && !restrictToAvailableHazardCharacterisations;

            if (ShouldCompute) {
                var useDoseResponseData = _project.EffectSettings.UseDoseResponseModels;
                var isHazardDoseImputation = _project.EffectSettings.ImputeMissingHazardDoses;
                var requireInVitro = _project.EffectSettings.TargetDosesCalculationMethod != TargetDosesCalculationMethod.InVivoPods;
                var requireKinetics = _project.EffectSettings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms
                    || (_project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal && _project.EffectSettings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVivoPods)
                    || (_project.EffectSettings.TargetDoseLevelType == TargetLevelType.External && _project.EffectSettings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.InVitroBmds);
                _actionInputRequirements[ActionType.KineticModels].IsRequired = false;
                _actionInputRequirements[ActionType.KineticModels].IsVisible = requireKinetics;
                _actionInputRequirements[ActionType.EffectRepresentations].IsVisible = useDoseResponseData || requireInVitro;
                _actionInputRequirements[ActionType.EffectRepresentations].IsRequired = useDoseResponseData || requireInVitro;
                _actionInputRequirements[ActionType.DoseResponseModels].IsVisible = useDoseResponseData || requireInVitro;
                _actionInputRequirements[ActionType.DoseResponseModels].IsRequired = useDoseResponseData || requireInVitro;
                _actionInputRequirements[ActionType.PointsOfDeparture].IsRequired = !isHazardDoseImputation && !useDoseResponseData;
                _actionInputRequirements[ActionType.IntraSpeciesFactors].IsVisible = _project.EffectSettings.UseIntraSpeciesConversionFactors;
                _actionInputRequirements[ActionType.IntraSpeciesFactors].IsRequired = false;
                _actionInputRequirements[ActionType.InterSpeciesConversions].IsVisible = _project.EffectSettings.UseInterSpeciesConversionFactors;
                _actionInputRequirements[ActionType.InterSpeciesConversions].IsRequired = false;
            } else {
                _actionDataLinkRequirements[ScopingType.HazardCharacterisations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
                _actionDataLinkRequirements[ScopingType.HazardCharacterisations][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
                _actionInputRequirements[ActionType.PointsOfDeparture].IsRequired = false;
                _actionInputRequirements[ActionType.IntraSpeciesFactors].IsVisible = false;
                _actionInputRequirements[ActionType.IntraSpeciesFactors].IsRequired = false;
                _actionInputRequirements[ActionType.InterSpeciesConversions].IsVisible = false;
                _actionInputRequirements[ActionType.InterSpeciesConversions].IsRequired = false;
            }
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = _project.EffectSettings.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = _project.EffectSettings.IncludeAopNetworks;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new HazardCharacterisationsSettingsManager();
        }


        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleRPFs) {
                result.Add(UncertaintySource.HazardCharacterisationsSelection);
                if (_project.EffectSettings.ImputeMissingHazardDoses) {
                    result.Add(UncertaintySource.HazardCharacterisationsImputation);
                }
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HazardCharacterisationsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var settings = new HazardCharacterisationsModuleSettings(_project);
            var targetHazardDoseType = settings.GetTargetHazardDoseType();
            var targetDoseLevel = settings.TargetDoseLevel;
            var compartment = targetDoseLevel == TargetLevelType.External ? "bw" : null;
            var targetDoseUnit = data.HazardCharacterisationsUnit ?? TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            var allHazardCharacterisations = subsetManager.AllHazardCharacterisations;
            var exposureRoutes = getExposureRoutes(settings.Aggregate);
            var targetDoseUnitConverter = new HazardDoseConverter(targetHazardDoseType, targetDoseUnit);
            var podLookup = data.PointsOfDeparture?.ToLookup(r => r.Code, StringComparer.OrdinalIgnoreCase);
            data.HazardCharacterisationsUnit = targetDoseUnit;
            data.ExposureRoutes = data.ExposureRoutes ?? exposureRoutes;
            data.HazardCharacterisations = allHazardCharacterisations
                .Where(r => r.ExposureType == settings.ExposureType)
                .Where(r => !settings.RestrictToCriticalEffect || r.IsCriticalEffect)
                .Where(r => r.TargetLevel == targetDoseLevel)
                .Where(r => r.TargetLevel == TargetLevelType.Internal || exposureRoutes.Contains(r.ExposureRoute))
                .Select(r => new HazardCharacterisationModel() {
                    Code = r.Code,
                    Effect = r.Effect,
                    Substance = r.Substance,
                    ExposureRoute = r.ExposureRoute,
                    Value = targetDoseUnitConverter.ConvertToTargetUnit(r.DoseUnit, r.Substance, r.Value),
                    DoseUnit = targetDoseUnit,
                    PotencyOrigin = findPotencyOrigin(podLookup, r),
                    TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                        Effect = r.Effect,
                    },
                    HazardCharacterisationType = r.HazardCharacterisationType,
                    Reference = new PublicationReference() {
                        PublicationAuthors = r.PublicationAuthors,
                        PublicationTitle = r.PublicationTitle,
                        PublicationUri = r.PublicationUri,
                        PublicationYear = r.PublicationYear
                    }
                })
                .ToDictionary(r => r.Substance, r => r as IHazardCharacterisationModel);

            // Check if sufficient data
            if (!data.HazardCharacterisations.Any()) {
                if (data.AllCompounds.Count == 1) {
                    throw new Exception($"No hazard characterisation available for {data.AllCompounds.First().Code}.");
                } else {
                    throw new Exception("Failed to compute hazard characterisations for all substances.");
                }
            }
        }

        private PotencyOrigin findPotencyOrigin(
            ILookup<string, Data.Compiled.Objects.PointOfDeparture> podLookup,
            HazardCharacterisation hazardCharacterisation
        ) {
            if (!string.IsNullOrEmpty(hazardCharacterisation.IdPointOfDeparture)
                && podLookup != null
                && podLookup.Contains(hazardCharacterisation.IdPointOfDeparture)
            ) {
                var items = podLookup[hazardCharacterisation.IdPointOfDeparture];
                var result = items.FirstOrDefault(pod => pod.Compound == hazardCharacterisation.Substance);
                if (result != null) {
                    return result.PointOfDepartureType.ToPotencyOrigin();
                }
            } else {
                return hazardCharacterisation.HazardCharacterisationType.ToPotencyOrigin();
            }
            return PotencyOrigin.Unknown;
        }

        protected override HazardCharacterisationsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new HazardCharacterisationsModuleSettings(_project);

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var targetPointOfDepartureType = settings.GetTargetHazardDoseType();
            var targetDoseLevel = settings.TargetDoseLevel;
            var compartment = targetDoseLevel == TargetLevelType.External ? "bw" : null;

            var unit = TargetUnit.CreateDietaryExposureUnit(data.ConsumptionUnit, data.ConcentrationUnit, data.BodyWeightUnit, false);
            var targetDoseUnit = data.HazardCharacterisationsUnit ?? new TargetUnit(unit.SubstanceAmount, unit.ConcentrationMassUnit, compartment, unit.TimeScaleUnit);
            targetDoseUnit.SetTimeScale(settings.TargetDoseLevel, settings.ExposureType);

            var exposureRoutes = getExposureRoutes(settings.Aggregate);
            var hazardDoseConverter = new HazardDoseConverter(targetPointOfDepartureType, targetDoseUnit);
            var kineticModelFactory = new KineticModelCalculatorFactory(
                data.AbsorptionFactors,
                data.KineticModelInstances
            );
            var kineticConversionFactorCalculator = new KineticConversionFactorCalculator(
                targetDoseLevel,
                kineticModelFactory,
                data.SelectedPopulation.NominalBodyWeight
            );
            var additionalAssessmentFactor = settings.UseAdditionalAssessmentFactor ? settings.AdditionalAssessmentFactor : 1;
            var interSpeciesFactorModels = settings.UseInterSpeciesConversionFactors ? data.InterSpeciesFactorModels : null;
            var intraSpeciesFactorModels = settings.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null;
            var targetDosesCalculator = new HazardCharacterisationsCalculator(
                interSpeciesFactorModels,
                intraSpeciesFactorModels,
                additionalAssessmentFactor,
                kineticConversionFactorCalculator,
                data.SelectedPopulation.NominalBodyWeight
            );

            // PoD random random generator
            var pointsOfDepartureRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : null; // In the nominal run, we don't need a random generator

            // Kinetic model random generator
            var kineticModelRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? new McraRandomGenerator(pointsOfDepartureRandomGenerator.Next(), true)
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HC_DrawKineticModelParameters));

            // Compute/collect available hazard characterisations
            localProgress.Update("Collecting available hazard characterisations");
            var hazardCharacterisationsFromPodAndBmd = targetDosesCalculator
                .CollectAvailableHazardCharacterisations(
                    substances,
                    data.ReferenceCompound,
                    data.PointsOfDeparture,
                    data.DoseResponseModels,
                    data.FocalEffectRepresentations,
                    settings.ExposureType,
                    settings.TargetDosesCalculationMethod,
                    hazardDoseConverter,
                    targetDoseUnit,
                    kineticModelRandomGenerator
                );

            // Get the selected target doses
            localProgress.Update("Selecting hazard characterisations");
            var aggregateHazardCharacterisationsCalculator = new AggregateHazardCharacterisationCalculator();
            var selectedHazardCharacterisations = aggregateHazardCharacterisationsCalculator
                .SelectTargetDoses(
                    substances,
                    data.SelectedEffect,
                    hazardCharacterisationsFromPodAndBmd,
                    settings.TargetDoseSelectionMethod == TargetDoseSelectionMethod.Draw
                        ? TargetDoseSelectionMethod.Aggregate 
                        : settings.TargetDoseSelectionMethod,
                    null
                );

            // Points Of Departure imputation
            var isIvive = settings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms;
            ICollection<IHazardCharacterisationModel> imputedHazardCharacterisations = null;
            ICollection<IHazardCharacterisationModel> hazardCharacterisationImputationRecords = null;
            if (settings.ImputeMissingHazardDoses) {
                var imputationMethod = settings.HazardDoseImputationMethod;
                var missingPointsOfDeparture = isIvive
                    ? data.ReferenceCompound
                        .ToSingleElementSequence()
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList()
                    : substances
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList();
                if (missingPointsOfDeparture.Any()) {
                    localProgress.Update("Imputing missing substance hazard characterisations");
                    var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
                            imputationMethod,
                            data.SelectedEffect,
                            hazardCharacterisationsFromPodAndBmd,
                            interSpeciesFactorModels,
                            kineticConversionFactorCalculator,
                            intraSpeciesFactorModels
                    );
                    imputedHazardCharacterisations = imputationCalculator
                        .ImputeNominal(
                            missingPointsOfDeparture,
                            hazardDoseConverter,
                            targetDoseUnit,
                            kineticModelRandomGenerator
                        );
                    hazardCharacterisationImputationRecords = imputationCalculator.GetImputationRecords();
                    foreach (var record in imputedHazardCharacterisations) {
                        selectedHazardCharacterisations.Add(record.Substance, record);
                    }
                }
            }

            // IVIVE
            var hazardDoseModelsForTimeCourse = selectedHazardCharacterisations.Values.ToHashSet();
            ICollection<IviveHazardCharacterisation> iviveTargetDoses = null;
            if (isIvive) {

                if (data.ReferenceCompound == null) {
                    throw new Exception("Reference substance is not specified, but required for IVIVE.");
                }
                if (!selectedHazardCharacterisations.TryGetValue(data.ReferenceCompound, out var referenceRecord)) {
                    throw new Exception("No available hazard characterisation for the reference substance.");
                }
                var iviveCalculator = new HazardCharacterisationsFromIviveCalculator();
                iviveTargetDoses = iviveCalculator
                    .Compute(
                        data.SelectedEffect,
                        data.DoseResponseModels,
                        substances,
                        referenceRecord,
                        data.FocalEffectRepresentations,
                        targetDoseUnit,
                        settings.ExposureType,
                        targetDoseLevel,
                        hazardDoseConverter,
                        interSpeciesFactorModels,
                        kineticConversionFactorCalculator,
                        intraSpeciesFactorModels,
                        additionalAssessmentFactor,
                        kineticModelRandomGenerator
                    );
                foreach (var record in iviveTargetDoses) {
                    if (settings.TargetDoseLevel == TargetLevelType.External) {
                        hazardDoseModelsForTimeCourse.Add(record);
                    }
                    if (record.Substance != referenceRecord.Substance) {
                        selectedHazardCharacterisations[record.Substance] = record;
                    }
                }

                // Another hazard dose imputation round
                if (settings.ImputeMissingHazardDoses) {
                    var missingPointsOfDeparture = substances.Where(r => !selectedHazardCharacterisations.ContainsKey(r)).ToList();
                    if (missingPointsOfDeparture.Any()) {
                        imputedHazardCharacterisations = imputedHazardCharacterisations ?? new List<IHazardCharacterisationModel>();
                        localProgress.Update("Imputing missing substance hazard characterisations");
                        var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
                                settings.HazardDoseImputationMethod,
                                data.SelectedEffect,
                                hazardCharacterisationsFromPodAndBmd,
                                interSpeciesFactorModels,
                                kineticConversionFactorCalculator,
                                intraSpeciesFactorModels
                        );
                        var imputationRecords = imputationCalculator
                            .ImputeNominal(
                                missingPointsOfDeparture,
                                hazardDoseConverter,
                                targetDoseUnit,
                                kineticModelRandomGenerator
                            );
                        foreach (var record in imputationRecords) {
                            imputedHazardCharacterisations.Add(record);
                            selectedHazardCharacterisations.Add(record.Substance, record);
                        }
                        hazardCharacterisationImputationRecords = hazardCharacterisationImputationRecords ?? imputationCalculator.GetImputationRecords();
                    }
                }
            }

            // Check if sufficient data
            if (!selectedHazardCharacterisations.Any()) {
                throw new Exception("Failed to compute hazard characterisations for all substances.");
            }

            // Kinetic model drilldown
            kineticModelRandomGenerator.Reset();
            var kineticModelDrilldownRecords = targetDosesCalculator
                .ComputeTargetDosesTimeCourses(
                    hazardDoseModelsForTimeCourse,
                    settings.ExposureType,
                    settings.TargetDoseLevel,
                    kineticModelFactory,
                    targetDoseUnit,
                    kineticModelRandomGenerator
                );
            localProgress.Update(100);

            return new HazardCharacterisationsActionResult() {
                HazardCharacterisationType = targetPointOfDepartureType,
                TargetDoseUnit = targetDoseUnit,
                HazardCharacterisations = selectedHazardCharacterisations,
                HazardCharacterisationsFromPodAndBmd = hazardCharacterisationsFromPodAndBmd,
                HazardCharacterisationsFromIvive = iviveTargetDoses,
                ImputedHazardCharacterisations = imputedHazardCharacterisations,
                HazardCharacterisationImputationRecords = hazardCharacterisationImputationRecords,
                KineticModelDrilldownRecords = kineticModelDrilldownRecords,
                ExposureRoutes = exposureRoutes
            };
        }

        private ICollection<ExposureRouteType> getExposureRoutes(bool isAggregate) {
            if (isAggregate) {
                return new List<ExposureRouteType> {
                    ExposureRouteType.Dietary,
                    ExposureRouteType.Dermal,
                    ExposureRouteType.Inhalation,
                    ExposureRouteType.Oral
                };
            } else {
                return new List<ExposureRouteType> {
                    ExposureRouteType.Dietary,
                };
            }
        }

        protected override void updateSimulationData(
            ActionData data, 
            HazardCharacterisationsActionResult result
        ) {
            data.HazardCharacterisations = result.HazardCharacterisations;
            data.HazardCharacterisationsUnit = result.TargetDoseUnit;
            data.ExposureRoutes = data.ExposureRoutes ?? result.ExposureRoutes;
        }

        protected override void summarizeActionResult(
            HazardCharacterisationsActionResult actionResult, 
            ActionData data, 
            SectionHeader header, 
            int order, 
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new HazardCharacterisationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override HazardCharacterisationsActionResult runUncertain(
            ActionData data, 
            UncertaintyFactorialSet factorialSet, 
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, 
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new HazardCharacterisationsModuleSettings(_project);

            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var targetDoseUnit = data.HazardCharacterisationsUnit;
            var targetHazardDoseType = settings.GetTargetHazardDoseType();
            var targetDoseLevel = settings.TargetDoseLevel;
            var hazardDoseConverter = new HazardDoseConverter(targetHazardDoseType, targetDoseUnit);
            var kineticModelFactory = new KineticModelCalculatorFactory(data.AbsorptionFactors, data.KineticModelInstances);
            var kineticConversionFactorCalculator = new KineticConversionFactorCalculator(
                targetDoseLevel,
                kineticModelFactory,
                data.SelectedPopulation.NominalBodyWeight
            );

            var additionalAssessmentFactor = settings.UseAdditionalAssessmentFactor ? settings.AdditionalAssessmentFactor : 1;
            var interSpeciesFactorModels = settings.UseInterSpeciesConversionFactors ? data.InterSpeciesFactorModels : null;
            var intraSpeciesFactorModels = settings.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null;
            var targetDosesCalculator = new HazardCharacterisationsCalculator(
                interSpeciesFactorModels,
                intraSpeciesFactorModels,
                additionalAssessmentFactor,
                kineticConversionFactorCalculator,
                data.SelectedPopulation.NominalBodyWeight
            );

            // Random generator for selection/aggregation of multiple HCs (uncertainty)
            var hazardCharacterisationsSelectionGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : factorialSet.Contains(UncertaintySource.HazardCharacterisationsSelection)
                    ? uncertaintySourceGenerators[UncertaintySource.HazardCharacterisationsSelection]
                    : null; // If the uncertainty source is not included in the factorial set, then we don't expect to need this generator

            // Random generator for HC imputation (uncertainty)
            var imputationRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? hazardCharacterisationsSelectionGenerator
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)UncertaintySource.HazardCharacterisationsImputation));

            // Random generator for kinetic models (variability)
            var kineticModelRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(hazardCharacterisationsSelectionGenerator.Next())
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.HC_DrawKineticModelParameters));

            // Compute/collect available hazard doses
            localProgress.Update("Collecting available hazard characterisations");
            var hazardCharacterisationsFromPodAndBmd = targetDosesCalculator
                .CollectAvailableHazardCharacterisations(
                    substances,
                    data.ReferenceCompound,
                    data.PointsOfDeparture,
                    data.DoseResponseModels,
                    data.FocalEffectRepresentations,
                    settings.ExposureType,
                    settings.TargetDosesCalculationMethod,
                    hazardDoseConverter,
                    targetDoseUnit,
                    kineticModelRandomGenerator
                );

            // Get the selected hazard doses
            localProgress.Update("Selecting hazard characterisations");
            var targetDoseSelectionMethod = factorialSet.Contains(UncertaintySource.HazardCharacterisationsSelection)
                ? TargetDoseSelectionMethod.Draw
                : settings.TargetDoseSelectionMethod;
            var aggregateHazardCharacterisationsCalculator = new AggregateHazardCharacterisationCalculator();
            var selectedTargetDoses = aggregateHazardCharacterisationsCalculator
                .SelectTargetDoses(
                    substances,
                    data.SelectedEffect,
                    hazardCharacterisationsFromPodAndBmd,
                    targetDoseSelectionMethod,
                    hazardCharacterisationsSelectionGenerator
                );

            // Hazard dose imputation
            var isIvive = settings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms;
            ICollection<IHazardCharacterisationModel> imputedHazardCharacterisations = null;
            if (settings.ImputeMissingHazardDoses) {
                var missingPointsOfDeparture = isIvive
                    ? data.ReferenceCompound
                        .ToSingleElementSequence()
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList()
                    : substances
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList();
                if (missingPointsOfDeparture.Any()) {
                    localProgress.Update("Imputing missing substance hazard characterisations");
                    var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
                            settings.HazardDoseImputationMethod,
                            data.SelectedEffect,
                            hazardCharacterisationsFromPodAndBmd,
                            interSpeciesFactorModels,
                            kineticConversionFactorCalculator,
                            intraSpeciesFactorModels
                    );
                    if (factorialSet.Contains(UncertaintySource.HazardCharacterisationsImputation)) {
                        imputedHazardCharacterisations = imputationCalculator.ImputeUncertaintyRun(
                            missingPointsOfDeparture,
                            hazardDoseConverter,
                            targetDoseUnit,
                            imputationRandomGenerator,
                            kineticModelRandomGenerator
                        );
                    } else {
                        imputedHazardCharacterisations = imputationCalculator.ImputeNominal(
                            missingPointsOfDeparture,
                            hazardDoseConverter,
                            targetDoseUnit,
                            kineticModelRandomGenerator);
                    }
                    foreach (var record in imputedHazardCharacterisations) {
                        selectedTargetDoses.Add(record.Substance, record);
                    }
                }
            }

            // IVIVE
            ICollection<IviveHazardCharacterisation> iviveTargetDoseModels = null;
            if (settings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms) {
                if (!selectedTargetDoses.TryGetValue(data.ReferenceCompound, out var referenceRecord)) {
                    throw new Exception("No available hazard characterisation for the reference substance.");
                }
                var iviveCalculator = new HazardCharacterisationsFromIviveCalculator();
                iviveTargetDoseModels = iviveCalculator.Compute(
                    data.SelectedEffect,
                    data.DoseResponseModels,
                    substances,
                    referenceRecord,
                    data.FocalEffectRepresentations,
                    targetDoseUnit,
                    settings.ExposureType,
                    targetDoseLevel,
                    hazardDoseConverter,
                    interSpeciesFactorModels,
                    kineticConversionFactorCalculator,
                    intraSpeciesFactorModels,
                    additionalAssessmentFactor,
                    kineticModelRandomGenerator
                );
                foreach (var record in iviveTargetDoseModels) {
                    if (record.Substance != referenceRecord.Substance) {
                        selectedTargetDoses[record.Substance] = record;
                    }
                }
            }

            // Another hazard dose imputation round
            if (settings.ImputeMissingHazardDoses) {
                var missingHazardDoses = substances.Where(r => !selectedTargetDoses.ContainsKey(r)).ToList();
                if (missingHazardDoses.Any()) {
                    imputedHazardCharacterisations = imputedHazardCharacterisations ?? new List<IHazardCharacterisationModel>();
                    localProgress.Update("Imputing missing substance hazard characterisations");
                    var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
                            settings.HazardDoseImputationMethod,
                            data.SelectedEffect,
                            hazardCharacterisationsFromPodAndBmd,
                            interSpeciesFactorModels,
                            kineticConversionFactorCalculator,
                            intraSpeciesFactorModels
                    );
                    ICollection<IHazardCharacterisationModel> imputationRecords;
                    if (factorialSet.Contains(UncertaintySource.HazardCharacterisationsImputation)) {
                        imputationRecords = imputationCalculator.ImputeUncertaintyRun(
                            missingHazardDoses,
                            hazardDoseConverter,
                            targetDoseUnit,
                            imputationRandomGenerator,
                            kineticModelRandomGenerator);
                    } else {
                        imputationRecords = imputationCalculator.ImputeNominal(
                            missingHazardDoses,
                            hazardDoseConverter,
                            targetDoseUnit,
                            kineticModelRandomGenerator);
                    }
                    foreach (var record in imputationRecords) {
                        selectedTargetDoses.Add(record.Substance, record);
                        imputedHazardCharacterisations.Add(record);
                    }
                }
            }

            localProgress.Update(100);

            return new HazardCharacterisationsActionResult() {
                HazardCharacterisationType = targetHazardDoseType,
                HazardCharacterisations = selectedTargetDoses,
                HazardCharacterisationsFromPodAndBmd = hazardCharacterisationsFromPodAndBmd,
                ImputedHazardCharacterisations = imputedHazardCharacterisations,
                TargetDoseUnit = targetDoseUnit,
                HazardCharacterisationsFromIvive = iviveTargetDoseModels,
            };
        }

        protected override void updateSimulationDataUncertain(ActionData data, HazardCharacterisationsActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, HazardCharacterisationsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new HazardCharacterisationsSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, HazardCharacterisationsActionResult result) {
            var outputWriter = new HazardCharacterisationsOutputWriter();
            outputWriter.WriteOutputData(_project, data, rawDataWriter);
        }
    }
}
