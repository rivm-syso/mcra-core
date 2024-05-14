using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
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
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
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
                _actionDataLinkRequirements[ScopingType.HCSubgroups][ScopingType.HazardCharacterisations].AlertTypeMissingData = AlertType.Notification;
                _actionDataLinkRequirements[ScopingType.HCSubgroups][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
                _actionDataLinkRequirements[ScopingType.HCSubgroupsUncertain][ScopingType.HazardCharacterisations].AlertTypeMissingData = AlertType.Notification;
                _actionDataLinkRequirements[ScopingType.HazardCharacterisationsUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
                _actionInputRequirements[ActionType.PointsOfDeparture].IsRequired = false;
                _actionInputRequirements[ActionType.PointsOfDeparture].IsVisible = false;
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
                result.Add(UncertaintySource.HazardCharacterisations);
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
            var substances = settings.RestrictToAvailableHazardCharacterisations
                ? data.AllCompounds : data.ActiveSubstances;
            var podLookup = data.PointsOfDeparture?.ToLookup(r => r.Code, StringComparer.OrdinalIgnoreCase);
            data.HazardCharacterisationModelsCollections = subsetManager.AllHazardCharacterisations
                .Where(r => substances.Contains(r.Substance))
                .GroupBy(c => CreateExposureTargetKey(c))
                .Select(hc => {
                    // MCRA expresses the hazard characterisaiton values in a default unit, so imported values may
                    // be scaled to match these default units.
                    var targetUnit = getDefaultTargetUnit(settings.TargetDoseLevel, data, hc.Key);
                    var hazardDoseConverter = new HazardDoseConverter(targetUnit.ExposureUnit);
                    return new HazardCharacterisationModelCompoundsCollection {
                        TargetUnit = targetUnit,
                        HazardCharacterisationModels = loadHazardCharacterisationsFromData(
                            hc,
                            settings,
                            targetUnit.ExposureUnit,
                            hazardDoseConverter,
                            podLookup,
                            _project.EffectSettings.HCSubgroupDependent
                        )
                    };
                })
                .OrderBy(r => r.TargetUnit.BiologicalMatrix)
                .ThenBy(r => r.TargetUnit.ExpressionType)
                .ToList();
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.HazardCharacterisations) && data.HazardCharacterisationModelsCollections != null) {
                localProgress.Update("Resampling points of departure.");
                data.HazardCharacterisationModelsCollections = resampleHazardCharacterisations(
                    data.HazardCharacterisationModelsCollections,
                    uncertaintySourceGenerators[UncertaintySource.HazardCharacterisations]
                    );
            }
            localProgress.Update(100);
        }

        protected override HazardCharacterisationsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new HazardCharacterisationsModuleSettings(_project);

            var hazardCharacterisationsActionResult = new HazardCharacterisationsActionResult();

            localProgress.Update("Collecting available hazard characterisations");
            var referenceSubstance = data.ActiveSubstances?
               .FirstOrDefault(c => c.Code.Equals(settings.CodeReferenceSubstance, StringComparison.OrdinalIgnoreCase));

            var exposureTargets = GetExposureTargets(data, settings);
            foreach (var exposureTarget in exposureTargets) {
                var targetUnit = getDefaultTargetUnit(settings.TargetDoseLevel, data, exposureTarget);
                var hazardDoseConverter = new HazardDoseConverter(
                    settings.GetTargetHazardDoseType(),
                    targetUnit.ExposureUnit
                );
                computeHazardCharacterisations(
                    settings,
                    targetUnit,
                    hazardDoseConverter,
                    data,
                    referenceSubstance,
                    ref hazardCharacterisationsActionResult
                );
            }

            hazardCharacterisationsActionResult.ExposureRoutes = getExposureRoutes(settings);
            hazardCharacterisationsActionResult.ReferenceSubstance = referenceSubstance;
            hazardCharacterisationsActionResult.HazardCharacterisationType = settings.GetTargetHazardDoseType();
            hazardCharacterisationsActionResult.TargetDoseUnit = hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections?.FirstOrDefault()?.TargetUnit;

            localProgress.Update(100);

            return hazardCharacterisationsActionResult;
        }

        protected override void updateSimulationData(
            ActionData data,
            HazardCharacterisationsActionResult result
        ) {
            data.HazardCharacterisationModelsCollections = result.HazardCharacterisationModelsCollections;
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

            var hazardCharacterisationsActionResult = new HazardCharacterisationsActionResult();

            var referenceSubstance = data.ActiveSubstances?
               .FirstOrDefault(c => c.Code.Equals(settings.CodeReferenceSubstance, StringComparison.OrdinalIgnoreCase));

            localProgress.Update("Collecting available hazard characterisations");

            var exposureTargets = GetExposureTargets(data, settings);
            foreach (var exposureTarget in exposureTargets) {
                var targetUnit = getDefaultTargetUnit(settings.TargetDoseLevel, data, exposureTarget);
                var hazardDoseConverter = new HazardDoseConverter(
                    settings.GetTargetHazardDoseType(),
                    targetUnit.ExposureUnit
                    );
                computeHazardCharacterisations(
                    settings,
                    targetUnit,
                    hazardDoseConverter,
                    data,
                    referenceSubstance,
                    ref hazardCharacterisationsActionResult,
                    factorialSet,
                    uncertaintySourceGenerators
                );
            }

            hazardCharacterisationsActionResult.HazardCharacterisationType = settings.GetTargetHazardDoseType();
            hazardCharacterisationsActionResult.ExposureRoutes = getExposureRoutes(settings);
            hazardCharacterisationsActionResult.TargetDoseUnit = hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections?.FirstOrDefault()?.TargetUnit;
            hazardCharacterisationsActionResult.ReferenceSubstance = referenceSubstance;

            localProgress.Update(100);

            return hazardCharacterisationsActionResult;
        }

        private void computeHazardCharacterisations(
            HazardCharacterisationsModuleSettings settings,
            TargetUnit targetUnit,
            HazardDoseConverter hazardDoseConverter,
            ActionData data,
            Compound referenceSubstance,
            ref HazardCharacterisationsActionResult hazardCharacterisationsActionResult,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var targetPointOfDeparture = settings.GetTargetHazardDoseType();

            var kineticModelFactory = new KineticModelCalculatorFactory(
                data.AbsorptionFactors,
                data.KineticModelInstances
            );
            var kineticConversionFactorCalculator = new KineticConversionFactorCalculator(
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

            // Random generator for kinetic models (variability)
            var kineticModelRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed,
                (int)RandomSource.HC_DrawKineticModelParameters)
            );

            // TODO: this part is a remnant after the refactoring of code that combined the nominal run with the run uncertain.
            //       There was a slight difference in the selected reference substance as shown below. This different is kept but
            //       might possibly be wrong and the same reference substance should be taken.
            var refSubstanceRun = data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : referenceSubstance;
            var refSubstanceRunUncertain = referenceSubstance;
            Compound refSubstance = null;
            if (factorialSet == null) {
                refSubstance = refSubstanceRun;
            } else {
                refSubstance = refSubstanceRunUncertain;
            }

            // Compute/collect available hazard doses
            var hazardCharacterisationsFromPodAndBmd = targetDosesCalculator
                .CollectAvailableHazardCharacterisations(
                    substances,
                    refSubstance,
                    data.PointsOfDeparture,
                    data.DoseResponseModels,
                    data.FocalEffectRepresentations,
                    settings.ExposureType,
                    settings.TargetDosesCalculationMethod,
                    settings.ConvertToSingleTargetMatrix,
                    hazardDoseConverter,
                    targetUnit,
                    settings.UseBMDL,
                    kineticModelRandomGenerator
                );

            // Random generator for selection/aggregation of multiple HCs (uncertainty)
            var hazardCharacterisationsSelectionGenerator = factorialSet?.Contains(UncertaintySource.HazardCharacterisationsSelection) == true
                ? uncertaintySourceGenerators[UncertaintySource.HazardCharacterisationsSelection]
                : null; // If the uncertainty source is not included in the factorial set, then we don't expect to need this generator

            // Get the selected hazard doses
            var targetDoseSelectionMethod = factorialSet?.Contains(UncertaintySource.HazardCharacterisationsSelection) == true
                ? TargetDoseSelectionMethod.Draw
                : settings.TargetDoseSelectionMethod;
            var aggregateHazardCharacterisationsCalculator = new AggregateHazardCharacterisationCalculator();
            var selectedHazardCharacterisations = aggregateHazardCharacterisationsCalculator
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
            ICollection<IHazardCharacterisationModel> hazardCharacterisationImputationRecords = null;

            // Random generator for HC imputation (uncertainty)
            var imputationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)UncertaintySource.HazardCharacterisationsImputation));

            // Impute when:
            // 1. target level is external
            // 2. target level is internal and conversion to single matrix is enabled
            if (settings.ImputeMissingHazardDoses
                && !(settings.TargetDoseLevel == TargetLevelType.Internal && !settings.ConvertToSingleTargetMatrix)
            ) {
                var imputationMethod = settings.HazardDoseImputationMethod;
                var missingPointsOfDeparture = isIvive
                    ? referenceSubstance
                        .ToSingleElementSequence()
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList()
                    : substances
                        .Where(r => !hazardCharacterisationsFromPodAndBmd
                        .Any(h => h.Substance == r))
                        .ToList();
                if (missingPointsOfDeparture.Any()) {
                    var imputationCalculator = HazardCharacterisationImputationCalculatorFactory
                        .Create(
                            imputationMethod,
                            data.SelectedEffect,
                            hazardCharacterisationsFromPodAndBmd,
                            interSpeciesFactorModels,
                            kineticConversionFactorCalculator,
                            intraSpeciesFactorModels
                    );
                    if (factorialSet?.Contains(UncertaintySource.HazardCharacterisationsImputation) == true) {
                        imputedHazardCharacterisations = imputationCalculator.ImputeUncertaintyRun(
                            missingPointsOfDeparture,
                            hazardDoseConverter,
                            targetUnit,
                            imputationRandomGenerator,
                            kineticModelRandomGenerator
                        );
                    } else {
                        imputedHazardCharacterisations = imputationCalculator.ImputeNominal(
                            missingPointsOfDeparture,
                            hazardDoseConverter,
                            targetUnit,
                            kineticModelRandomGenerator);
                    }
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
                if (referenceSubstance == null) {
                    throw new Exception("Reference substance is not specified, but required for IVIVE.");
                }
                if (!selectedHazardCharacterisations.TryGetValue(referenceSubstance, out var referenceRecord)) {
                    throw new Exception("No available hazard characterisation for the reference substance.");
                }
                var iviveCalculator = new HazardCharacterisationsFromIviveCalculator();
                iviveTargetDoses = iviveCalculator.Compute(
                    data.SelectedEffect,
                    data.DoseResponseModels,
                    substances,
                    referenceRecord,
                    data.FocalEffectRepresentations,
                    targetUnit,
                    settings.ExposureType,
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
            }

            // Another hazard dose imputation round
            if (settings.ImputeMissingHazardDoses) {
                var missingHazardDoses = substances.Where(r => !selectedHazardCharacterisations.ContainsKey(r)).ToList();
                if (missingHazardDoses.Any()) {
                    imputedHazardCharacterisations = imputedHazardCharacterisations ?? new List<IHazardCharacterisationModel>();
                    var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
                            settings.HazardDoseImputationMethod,
                            data.SelectedEffect,
                            hazardCharacterisationsFromPodAndBmd,
                            interSpeciesFactorModels,
                            kineticConversionFactorCalculator,
                            intraSpeciesFactorModels
                    );
                    ICollection<IHazardCharacterisationModel> imputationRecords;
                    if (factorialSet?.Contains(UncertaintySource.HazardCharacterisationsImputation) == true) {
                        imputationRecords = imputationCalculator.ImputeUncertaintyRun(
                            missingHazardDoses,
                            hazardDoseConverter,
                            targetUnit,
                            imputationRandomGenerator,
                            kineticModelRandomGenerator);
                    } else {
                        imputationRecords = imputationCalculator.ImputeNominal(
                            missingHazardDoses,
                            hazardDoseConverter,
                            targetUnit,
                            kineticModelRandomGenerator);
                    }
                    foreach (var record in imputationRecords) {
                        imputedHazardCharacterisations.Add(record);
                        selectedHazardCharacterisations.Add(record.Substance, record);
                    }

                    if (factorialSet == null) {
                        hazardCharacterisationImputationRecords = hazardCharacterisationImputationRecords ?? imputationCalculator.GetImputationRecords();
                    }
                }
            }

            var kineticModelDrilldownRecords = new List<(AggregateIndividualExposure, IHazardCharacterisationModel)>();
            if (factorialSet == null) {
                // Kinetic model drilldown
                kineticModelRandomGenerator.Reset();
                kineticModelDrilldownRecords = targetDosesCalculator
                    .ComputeTargetDosesTimeCourses(
                        hazardDoseModelsForTimeCourse,
                        settings.ExposureType,
                        kineticModelFactory,
                        targetUnit,
                        kineticModelRandomGenerator
                    );
            }

            UpdateActionResult(
                targetUnit,
                hazardCharacterisationsFromPodAndBmd,
                selectedHazardCharacterisations,
                imputedHazardCharacterisations,
                hazardCharacterisationImputationRecords,
                iviveTargetDoses,
                kineticModelDrilldownRecords,
                ref hazardCharacterisationsActionResult
            );
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

        private ICollection<ExposureRoute> getExposureRoutes(HazardCharacterisationsModuleSettings settings) {
            if (settings.TargetDoseLevel == TargetLevelType.External) {
                if (settings.Aggregate) {
                    return new List<ExposureRoute> {
                        ExposureRoute.Dermal,
                        ExposureRoute.Inhalation,
                        ExposureRoute.Oral
                    };
                } else {
                    return new List<ExposureRoute> { ExposureRoute.Oral };
                }
            } else {
                return new List<ExposureRoute> { ExposureRoute.Undefined };
            }
        }

        private IDictionary<Compound, IHazardCharacterisationModel> loadHazardCharacterisationsFromData(
           IEnumerable<HazardCharacterisation> hazardCharacterisations,
           HazardCharacterisationsModuleSettings settings,
           ExposureUnitTriple exposureUnit,
           HazardDoseConverter hazardDoseConverter,
           ILookup<string, Data.Compiled.Objects.PointOfDeparture> podLookup,
           bool hcSubgroupDependent
       ) {
            var targetLevel = settings.TargetDoseLevel;
            var exposureRoutes = getExposureRoutes(settings);
            var hazardCharacterisationModels = hazardCharacterisations
                    .Where(r => r.ExposureType == settings.ExposureType)
                    .Where(r => !settings.RestrictToCriticalEffect || r.IsCriticalEffect)
                    .Where(r => r.TargetLevel == settings.TargetDoseLevel)
                    .Where(r => r.TargetLevel == TargetLevelType.Internal || exposureRoutes.Contains(r.ExposureRoute))
                    .Select(r => new HazardCharacterisationModel() {
                        Code = r.Code,
                        Effect = r.Effect,
                        Substance = r.Substance,
                        TargetUnit = new TargetUnit(
                            targetLevel == TargetLevelType.External
                                ? new ExposureTarget(r.ExposureRoute) 
                                : new ExposureTarget(r.BiologicalMatrix, r.ExpressionType),
                            exposureUnit
                            ),
                        Value = hazardDoseConverter.ConvertToTargetUnit(r.DoseUnit, r.Substance, r.Value),
                        PotencyOrigin = findPotencyOrigin(podLookup, r),
                        TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() { Effect = r.Effect },
                        HazardCharacterisationType = r.HazardCharacterisationType,
                        HazardCharacterisationsUncertains = r.HazardCharacterisationsUncertains.Select(u => {
                            return new HazardCharacterisationUncertain {
                                Substance = u.Substance,
                                IdHazardCharacterisation = u.IdHazardCharacterisation,
                                Value = hazardDoseConverter.ConvertToTargetUnit(r.DoseUnit, u.Substance, u.Value)
                            };
                        }).ToList(),
                        Reference = new PublicationReference() {
                            PublicationAuthors = r.PublicationAuthors,
                            PublicationTitle = r.PublicationTitle,
                            PublicationUri = r.PublicationUri,
                            PublicationYear = r.PublicationYear
                        },
                        HCSubgroups = hcSubgroupDependent ? r.HCSubgroups.OrderBy(c => c.AgeLower).ToList() : null,
                    })
                    .ToDictionary(r => r.Substance, r => r as IHazardCharacterisationModel);

            return hazardCharacterisationModels;
        }


        /// <summary>
        /// Resample HCSubgroup uncertainty first, than hazard characterisation uncertainty, 
        /// the last is used as default for missing hazard characterisation subsgroups
        /// </summary>
        private ICollection<HazardCharacterisationModelCompoundsCollection> resampleHazardCharacterisations(
          ICollection<HazardCharacterisationModelCompoundsCollection> hazardCharacterisationCollections,
          IRandom generator
        ) {
            var result = new List<HazardCharacterisationModelCompoundsCollection>();
            foreach (var collection in hazardCharacterisationCollections) {
                var models = collection.HazardCharacterisationModels;
                var resampledModels = new Dictionary<Compound, IHazardCharacterisationModel>();
                foreach (var model in models) {
                    var hcSubgroups = new List<HCSubgroup>();
                    var sampled = model.Value.Clone();
                    bool hasHCSubgroups = model.Value.HCSubgroups?.Any() ?? false;
                    if (hasHCSubgroups) {
                        foreach (var subgroup in model.Value.HCSubgroups) {
                            if (subgroup.HCSubgroupsUncertains?.Any() ?? false) {
                                var ix = generator.Next(0, subgroup.HCSubgroupsUncertains.Count);
                                var sampledUncertaintySubGroup = subgroup.HCSubgroupsUncertains.ElementAt(ix);
                                var sampledSubgroup = subgroup.Clone();
                                sampledSubgroup.Value = sampledUncertaintySubGroup.Value;
                                hcSubgroups.Add(sampledSubgroup);
                            } else {
                                hcSubgroups.Add(subgroup);
                            }
                        }
                        sampled.HCSubgroups = hcSubgroups;
                    }
                    if (model.Value.HazardCharacterisationsUncertains.Any()) {
                        var ix = generator.Next(0, model.Value.HazardCharacterisationsUncertains.Count);
                        var sampledUncertaintyValue = model.Value.HazardCharacterisationsUncertains.ElementAt(ix);
                        sampled.Value = sampledUncertaintyValue.Value;
                    }
                    if (hasHCSubgroups || model.Value.HazardCharacterisationsUncertains.Any()) {
                        resampledModels.Add(model.Key, sampled);
                    } else {
                        resampledModels.Add(model.Key, model.Value);
                    }
                }
                result.Add(new HazardCharacterisationModelCompoundsCollection {
                    TargetUnit = collection.TargetUnit,
                    HazardCharacterisationModels = resampledModels,
                });
            }
            return result;
        }

        private TargetUnit getDefaultTargetUnit(
            TargetLevelType targetLevelType,
            ActionData data,
            ExposureTarget exposureTarget
        ) {
            if (targetLevelType == TargetLevelType.External) {
                // NOTE: the value for isPerPerson should come from the project settings, but this
                // is also related to the BodyWeightUnit settings. This needs to be looked into in more detail,
                // see https://git.wur.nl/Biometris/mcra-dev/MCRA-Issues/-/issues/1739
                return TargetUnit.CreateDietaryExposureUnit(
                    data.ConsumptionUnit,
                    McraUnitDefinitions.DefaultExternalConcentrationUnit,
                    data.BodyWeightUnit,
                    isPerPerson: false
                );
            } else {
                return new TargetUnit(exposureTarget, ExposureUnitTriple.FromExposureTarget(exposureTarget));
            };
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

        private ExposureTarget CreateExposureTargetKey(HazardCharacterisation hazardCharacterisation) {
            if (hazardCharacterisation.TargetLevel == TargetLevelType.External) {
                return new ExposureTarget(hazardCharacterisation.ExposureRoute);
            } else {
                return new ExposureTarget(hazardCharacterisation.BiologicalMatrix, hazardCharacterisation.ExpressionType);
            }
        }

        private List<ExposureTarget> GetExposureTargets(ActionData data, HazardCharacterisationsModuleSettings settings) {
            var exposureTargets = new List<ExposureTarget>();
            if (settings.TargetDoseLevel == TargetLevelType.External) {
                // When external, we currently assume dietary (oral) route
                exposureTargets.Add(ExposureTarget.DietaryExposureTarget);
            } else {
                if (settings.ConvertToSingleTargetMatrix) {
                    if (settings.TargetMatrix.IsUndefined()) {
                        exposureTargets.Add(ExposureTarget.DefaultInternalExposureTarget);
                    } else {
                        exposureTargets.Add(new ExposureTarget(settings.TargetMatrix, ExpressionType.None));
                    }
                } else {
                    if (data.PointsOfDeparture?.Any() == true) {
                        exposureTargets = data.PointsOfDeparture
                            .Select(p => p.BiologicalMatrix.IsUndefined() ? ExposureTarget.DefaultInternalExposureTarget : new ExposureTarget(p.BiologicalMatrix, p.ExpressionType))
                            .Where(r => r.TargetLevelType == TargetLevelType.Internal)
                            .Distinct()
                            .OrderBy(t => t.BiologicalMatrix)
                            .ThenBy(t => t.ExpressionType)
                            .ToList();
                    }
                    if (data.DoseResponseModels?.Any() == true) {
                        exposureTargets = exposureTargets
                            .Append(ExposureTarget.DefaultInternalExposureTarget)
                            .ToList();
                    }
                }
            }
            return exposureTargets;
        }

        private void UpdateActionResult(
           TargetUnit targetUnit,
           List<IHazardCharacterisationModel> hazardCharacterisationsFromPodAndBmd,
           Dictionary<Compound, IHazardCharacterisationModel> selectedHazardCharacterisations,
           ICollection<IHazardCharacterisationModel> imputedHazardCharacterisations,
           ICollection<IHazardCharacterisationModel> hazardCharacterisationImputationRecords,
           ICollection<IviveHazardCharacterisation> iviveTargetDoses,
           List<(AggregateIndividualExposure, IHazardCharacterisationModel)> kineticModelDrilldownRecords,
           ref HazardCharacterisationsActionResult hazardCharacterisationsActionResult
        ) {
            hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections.Add(new HazardCharacterisationModelCompoundsCollection {
                HazardCharacterisationModels = selectedHazardCharacterisations,
                TargetUnit = targetUnit,
            });
            hazardCharacterisationsActionResult.HazardCharacterisationsFromPodAndBmd.Add(new HazardCharacterisationModelsCollection {
                HazardCharacterisationModels = hazardCharacterisationsFromPodAndBmd,
                TargetUnit = targetUnit,
            });

            if (iviveTargetDoses != null) {
                hazardCharacterisationsActionResult.HazardCharacterisationsFromIvive.AddRange(iviveTargetDoses);
            }
            if (imputedHazardCharacterisations != null) {
                hazardCharacterisationsActionResult.ImputedHazardCharacterisations.AddRange(imputedHazardCharacterisations);
            }
            if (hazardCharacterisationImputationRecords != null) {
                hazardCharacterisationsActionResult.HazardCharacterisationImputationRecords.AddRange(hazardCharacterisationImputationRecords);
            }
            if (kineticModelDrilldownRecords != null) {
                hazardCharacterisationsActionResult.KineticModelDrilldownRecords.AddRange(kineticModelDrilldownRecords);
            }
        }
    }
}
