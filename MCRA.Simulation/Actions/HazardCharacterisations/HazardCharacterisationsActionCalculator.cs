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
            var podLookup = data.PointsOfDeparture?.ToLookup(r => r.Code, StringComparer.OrdinalIgnoreCase);
            data.HazardCharacterisationModelsCollections = subsetManager.AllHazardCharacterisations
                .GroupBy(c => CreateExposureTargetKey(c))
                .Select(hc => {
                    var targetUnit = createTargetUnit(settings.TargetDoseLevel, data, hc.Key);
                    var hazardDoseConverter = new HazardDoseConverter(settings.GetTargetHazardDoseType(), targetUnit.ExposureUnit);
                    return new HazardCharacterisationModelsCollection {
                        TargetUnit = targetUnit,
                        HazardCharacterisationModels = loadHazardCharacterisationsFromData(
                            hc,
                            settings,
                            targetUnit.ExposureUnit,
                            hazardDoseConverter,
                            podLookup
                        )
                    };
                })
                .OrderBy(r => r.TargetUnit.BiologicalMatrix)
                .ThenBy(r => r.TargetUnit.ExpressionType)
                .ToList();
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
                var targetUnit = createTargetUnit(settings.TargetDoseLevel, data, exposureTarget);
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
                    null,
                    null,
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
                var targetUnit = createTargetUnit(settings.TargetDoseLevel, data, exposureTarget);
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
                    factorialSet,
                    uncertaintySourceGenerators,
                    ref hazardCharacterisationsActionResult
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
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            ref HazardCharacterisationsActionResult hazardCharacterisationsActionResult
        ) {
            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var targetPointOfDeparture = settings.GetTargetHazardDoseType();

            var kineticModelFactory = new KineticModelCalculatorFactory(
                data.AbsorptionFactors, 
                data.KineticModelInstances
            );
            var kineticConversionFactorCalculator = new KineticConversionFactorCalculator(
                targetUnit.Target.TargetLevelType,
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
                    targetUnit.Target.TargetLevelType,
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

            var kineticModelDrilldownRecords = new List<AggregateIndividualExposure>();
            if (factorialSet == null) {
                // Kinetic model drilldown
                kineticModelRandomGenerator.Reset();
                kineticModelDrilldownRecords = targetDosesCalculator
                    .ComputeTargetDosesTimeCourses(
                        hazardDoseModelsForTimeCourse,
                        settings.ExposureType,
                        settings.TargetDoseLevel,
                        kineticModelFactory,
                        targetUnit,
                        kineticModelRandomGenerator
                    );
            }

            if (hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections == null) {
                hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections = new List<HazardCharacterisationModelsCollection>();
            }
            hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections.Add(new HazardCharacterisationModelsCollection {
                HazardCharacterisationModels = selectedHazardCharacterisations,
                TargetUnit = targetUnit,
            });

            hazardCharacterisationsActionResult.HazardCharacterisationsFromPodAndBmd = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationsFromPodAndBmd, hazardCharacterisationsFromPodAndBmd);
            hazardCharacterisationsActionResult.HazardCharacterisationsFromIvive = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationsFromIvive, iviveTargetDoses);
            hazardCharacterisationsActionResult.ImputedHazardCharacterisations = SafeConcat(hazardCharacterisationsActionResult.ImputedHazardCharacterisations, imputedHazardCharacterisations);
            hazardCharacterisationsActionResult.HazardCharacterisationImputationRecords = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationImputationRecords, hazardCharacterisationImputationRecords);
            hazardCharacterisationsActionResult.KineticModelDrilldownRecords = SafeConcat(hazardCharacterisationsActionResult.KineticModelDrilldownRecords, kineticModelDrilldownRecords).ToList();
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

        private ICollection<ExposureRouteType> getExposureRoutes(HazardCharacterisationsModuleSettings settings) {
            if (settings.TargetDoseLevel == TargetLevelType.External) {
                if (settings.Aggregate) {
                    return new List<ExposureRouteType> {
                        ExposureRouteType.Dietary,
                        ExposureRouteType.Dermal,
                        ExposureRouteType.Inhalation,
                        ExposureRouteType.Oral
                    };
                } else {
                    return new List<ExposureRouteType> { ExposureRouteType.Dietary };
                }
            } else {
                return new List<ExposureRouteType> { ExposureRouteType.AtTarget };
            }
        }

        private IDictionary<Compound, IHazardCharacterisationModel> loadHazardCharacterisationsFromData(
           IEnumerable<HazardCharacterisation> hazardCharacterisations,
           HazardCharacterisationsModuleSettings settings,
           ExposureUnitTriple exposureUnit,
           HazardDoseConverter hazardDoseConverter,
           ILookup<string, Data.Compiled.Objects.PointOfDeparture> podLookup
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
                        Target = targetLevel == TargetLevelType.External ? new ExposureTarget(r.ExposureRoute) : new ExposureTarget(r.BiologicalMatrix, r.ExpressionType),
                        Value = hazardDoseConverter.ConvertToTargetUnit(r.DoseUnit, r.Substance, r.Value),
                        DoseUnit = exposureUnit,
                        PotencyOrigin = findPotencyOrigin(podLookup, r),
                        TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() { Effect = r.Effect },
                        HazardCharacterisationType = r.HazardCharacterisationType,
                        Reference = new PublicationReference() {
                            PublicationAuthors = r.PublicationAuthors,
                            PublicationTitle = r.PublicationTitle,
                            PublicationUri = r.PublicationUri,
                            PublicationYear = r.PublicationYear
                        }
                    })
                    .ToDictionary(r => r.Substance, r => r as IHazardCharacterisationModel);

            return hazardCharacterisationModels;
        }

        //private IDictionary<Compound, IHazardCharacterisationModel> computeHazardCharacterisationsNominal(
        //   HazardCharacterisationsModuleSettings settings,
        //   TargetUnit targetUnit,
        //   HazardDoseConverter hazardDoseConverter,
        //   ActionData data,
        //   Compound referenceSubstance,
        //   ref HazardCharacterisationsActionResult hazardCharacterisationsActionResult
        //) {
        //    var substances = data.ActiveSubstances ?? data.AllCompounds;
        //    var targetPointOfDeparture = settings.GetTargetHazardDoseType();

        //    var kineticModelFactory = new KineticModelCalculatorFactory(
        //        data.AbsorptionFactors,
        //        data.KineticModelInstances
        //    );
        //    var kineticConversionFactorCalculator = new KineticConversionFactorCalculator(
        //        targetUnit.Target.TargetLevelType,
        //        kineticModelFactory,
        //        data.SelectedPopulation.NominalBodyWeight
        //    );
        //    var additionalAssessmentFactor = settings.UseAdditionalAssessmentFactor ? settings.AdditionalAssessmentFactor : 1;
        //    var interSpeciesFactorModels = settings.UseInterSpeciesConversionFactors ? data.InterSpeciesFactorModels : null;
        //    var intraSpeciesFactorModels = settings.UseIntraSpeciesConversionFactors ? data.IntraSpeciesFactorModels : null;
        //    var targetDosesCalculator = new HazardCharacterisationsCalculator(
        //        interSpeciesFactorModels,
        //        intraSpeciesFactorModels,
        //        additionalAssessmentFactor,
        //        kineticConversionFactorCalculator,
        //        data.SelectedPopulation.NominalBodyWeight
        //    );

        //    // Kinetic model random generator
        //    var kineticModelRandomGenerator = new McraRandomGenerator(
        //        RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, 
        //        (int)RandomSource.HC_DrawKineticModelParameters)
        //    );

        //    // Collect available hazard characterisations
        //    // TODO: only collect for the current target and don't do kinetic conversion at this stage
        //    var hazardCharacterisationsFromPodAndBmd = targetDosesCalculator
        //        .CollectAvailableHazardCharacterisations(
        //            substances,
        //            data.ActiveSubstances.Count == 1 ? data.ActiveSubstances.First() : referenceSubstance,
        //            data.PointsOfDeparture,
        //            data.DoseResponseModels,
        //            data.FocalEffectRepresentations,
        //            settings.ExposureType,
        //            settings.TargetDosesCalculationMethod,
        //            settings.ConvertToSingleMatrix,
        //            hazardDoseConverter,
        //            targetUnit,
        //            kineticModelRandomGenerator
        //        );

        //    // Get the selected target doses
        //    var targetDoseSelectionMethod = settings.TargetDoseSelectionMethod == TargetDoseSelectionMethod.Draw
        //                ? TargetDoseSelectionMethod.Aggregate
        //                : settings.TargetDoseSelectionMethod;
        //    var aggregateHazardCharacterisationsCalculator = new AggregateHazardCharacterisationCalculator();
        //    var selectedHazardCharacterisations = aggregateHazardCharacterisationsCalculator
        //        .SelectTargetDoses(
        //            substances,
        //            data.SelectedEffect,
        //            hazardCharacterisationsFromPodAndBmd,
        //            targetDoseSelectionMethod,
        //            null
        //        );

        //    // Points Of Departure imputation
        //    var isIvive = settings.TargetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms;
        //    ICollection<IHazardCharacterisationModel> imputedHazardCharacterisations = null;
        //    ICollection<IHazardCharacterisationModel> hazardCharacterisationImputationRecords = null;

        //    // Impute when:
        //    // 1. target level is external
        //    // 2. target level is internal and conversion to single matrix is enabled
        //    if (settings.ImputeMissingHazardDoses
        //        && !(settings.TargetDoseLevel == TargetLevelType.Internal && !settings.ConvertToSingleMatrix)
        //    ) {
        //        var imputationMethod = settings.HazardDoseImputationMethod;
        //        var missingPointsOfDeparture = isIvive
        //            ? referenceSubstance
        //                .ToSingleElementSequence()
        //                .Where(r => !hazardCharacterisationsFromPodAndBmd
        //                .Any(h => h.Substance == r))
        //                .ToList()
        //            : substances
        //                .Where(r => !hazardCharacterisationsFromPodAndBmd
        //                .Any(h => h.Substance == r))
        //                .ToList();
        //        if (missingPointsOfDeparture.Any()) {
        //            var imputationCalculator = HazardCharacterisationImputationCalculatorFactory
        //                .Create(
        //                    imputationMethod,
        //                    data.SelectedEffect,
        //                    hazardCharacterisationsFromPodAndBmd,
        //                    interSpeciesFactorModels,
        //                    kineticConversionFactorCalculator,
        //                    intraSpeciesFactorModels
        //                );
        //            imputedHazardCharacterisations = imputationCalculator
        //                .ImputeNominal(
        //                    missingPointsOfDeparture,
        //                    hazardDoseConverter,
        //                    targetUnit,
        //                    kineticModelRandomGenerator
        //                );
        //            hazardCharacterisationImputationRecords = imputationCalculator.GetImputationRecords();
        //            foreach (var record in imputedHazardCharacterisations) {
        //                selectedHazardCharacterisations.Add(record.Substance, record);
        //            }
        //        }
        //    }

        //    // IVIVE
        //    var hazardDoseModelsForTimeCourse = selectedHazardCharacterisations.Values.ToHashSet();
        //    ICollection<IviveHazardCharacterisation> iviveTargetDoses = null;
        //    if (isIvive) {
        //        if (referenceSubstance == null) {
        //            throw new Exception("Reference substance is not specified, but required for IVIVE.");
        //        }
        //        if (!selectedHazardCharacterisations.TryGetValue(referenceSubstance, out var referenceRecord)) {
        //            throw new Exception("No available hazard characterisation for the reference substance.");
        //        }
        //        var iviveCalculator = new HazardCharacterisationsFromIviveCalculator();
        //        iviveTargetDoses = iviveCalculator
        //            .Compute(
        //                data.SelectedEffect,
        //                data.DoseResponseModels,
        //                substances,
        //                referenceRecord,
        //                data.FocalEffectRepresentations,
        //                targetUnit,
        //                settings.ExposureType,
        //                settings.TargetDoseLevel,
        //                hazardDoseConverter,
        //                interSpeciesFactorModels,
        //                kineticConversionFactorCalculator,
        //                intraSpeciesFactorModels,
        //                additionalAssessmentFactor,
        //                kineticModelRandomGenerator
        //            );
        //        foreach (var record in iviveTargetDoses) {
        //            if (settings.TargetDoseLevel == TargetLevelType.External) {
        //                hazardDoseModelsForTimeCourse.Add(record);
        //            }
        //            if (record.Substance != referenceRecord.Substance) {
        //                selectedHazardCharacterisations[record.Substance] = record;
        //            }
        //        }

        //        // Another hazard dose imputation round
        //        if (settings.ImputeMissingHazardDoses) {
        //            var missingPointsOfDeparture = substances.Where(r => !selectedHazardCharacterisations.ContainsKey(r)).ToList();
        //            if (missingPointsOfDeparture.Any()) {
        //                imputedHazardCharacterisations = imputedHazardCharacterisations ?? new List<IHazardCharacterisationModel>();
        //                //localProgress.Update("Imputing missing substance hazard characterisations");
        //                var imputationCalculator = HazardCharacterisationImputationCalculatorFactory.Create(
        //                        settings.HazardDoseImputationMethod,
        //                        data.SelectedEffect,
        //                        hazardCharacterisationsFromPodAndBmd,
        //                        interSpeciesFactorModels,
        //                        kineticConversionFactorCalculator,
        //                        intraSpeciesFactorModels
        //                );
        //                var imputationRecords = imputationCalculator
        //                    .ImputeNominal(
        //                        missingPointsOfDeparture,
        //                        hazardDoseConverter,
        //                        targetUnit,
        //                        kineticModelRandomGenerator
        //                    );
        //                foreach (var record in imputationRecords) {
        //                    imputedHazardCharacterisations.Add(record);
        //                    selectedHazardCharacterisations.Add(record.Substance, record);
        //                }
        //                hazardCharacterisationImputationRecords = hazardCharacterisationImputationRecords ?? imputationCalculator.GetImputationRecords();
        //            }
        //        }
        //    }

        //    // Kinetic model drilldown
        //    kineticModelRandomGenerator.Reset();
        //    var kineticModelDrilldownRecords = targetDosesCalculator
        //        .ComputeTargetDosesTimeCourses(
        //            hazardDoseModelsForTimeCourse,
        //            settings.ExposureType,
        //            settings.TargetDoseLevel,
        //            kineticModelFactory,
        //            targetUnit,
        //            kineticModelRandomGenerator
        //        );

        //    if (hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections == null) {
        //        hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections = new List<HazardCharacterisationModelsCollection>();
        //    }
        //    hazardCharacterisationsActionResult.HazardCharacterisationModelsCollections.Add(new HazardCharacterisationModelsCollection {
        //        HazardCharacterisationModels = selectedHazardCharacterisations,
        //        TargetUnit = targetUnit,
        //    });

        //    hazardCharacterisationsActionResult.HazardCharacterisationsFromPodAndBmd = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationsFromPodAndBmd, hazardCharacterisationsFromPodAndBmd);
        //    hazardCharacterisationsActionResult.HazardCharacterisationsFromIvive = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationsFromIvive, iviveTargetDoses);
        //    hazardCharacterisationsActionResult.ImputedHazardCharacterisations = SafeConcat(hazardCharacterisationsActionResult.ImputedHazardCharacterisations, imputedHazardCharacterisations);
        //    hazardCharacterisationsActionResult.HazardCharacterisationImputationRecords = SafeConcat(hazardCharacterisationsActionResult.HazardCharacterisationImputationRecords, hazardCharacterisationImputationRecords);
        //    hazardCharacterisationsActionResult.KineticModelDrilldownRecords = SafeConcat(hazardCharacterisationsActionResult.KineticModelDrilldownRecords, kineticModelDrilldownRecords).ToList();

        //    return selectedHazardCharacterisations;
        //}

        private TargetUnit createTargetUnit(
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
                return new TargetUnit(exposureTarget, McraUnitDefinitions.GetDefaultInternalTargetExposureUnit(exposureTarget.ExpressionType));
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
                exposureTargets.Add(new ExposureTarget(ExposureRouteType.Dietary));
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

        private ICollection<T> SafeConcat<T>(IEnumerable<T> first, IEnumerable<T> second) {
            if (first == null) {
                first = second;
            } else {
                first = first.Concat(second);
            }
            return first?.ToList();
        }
    }
}
