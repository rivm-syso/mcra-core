using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromPodCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ActiveSubstances {

    [ActionType(ActionType.ActiveSubstances)]
    public class ActiveSubstancesActionCalculator : ActionCalculatorBase<ActiveSubstancesActionResult> {
        private ActiveSubstancesModuleConfig ModuleConfig => (ActiveSubstancesModuleConfig)_moduleSettings;

        public ActiveSubstancesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var showEffects = !ShouldCompute
                || ModuleConfig.UseMolecularDockingModels
                || ModuleConfig.UseQsarModels
                || ModuleConfig.FilterByAvailableHazardDose;

            if (!IsLoopScope(ScopingType.ActiveSubstancesModels)) {
                _actionDataSelectionRequirements[ScopingType.ActiveSubstancesModels].MaxSelectionCount = 1;
            }

            _actionInputRequirements[ActionType.Effects].IsVisible = showEffects;
            _actionInputRequirements[ActionType.Effects].IsRequired = showEffects;
            _actionInputRequirements[ActionType.MolecularDockingModels].IsVisible = ShouldCompute && ModuleConfig.UseMolecularDockingModels;
            _actionInputRequirements[ActionType.MolecularDockingModels].IsRequired = ShouldCompute && ModuleConfig.UseMolecularDockingModels;
            _actionInputRequirements[ActionType.QsarMembershipModels].IsVisible = ShouldCompute && ModuleConfig.UseQsarModels;
            _actionInputRequirements[ActionType.QsarMembershipModels].IsRequired = ShouldCompute && ModuleConfig.UseQsarModels;
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = ModuleConfig.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = ModuleConfig.IncludeAopNetworks;
            _actionInputRequirements[ActionType.PointsOfDeparture].IsRequired = ModuleConfig.FilterByAvailableHazardDose;
            _actionInputRequirements[ActionType.PointsOfDeparture].IsVisible = ModuleConfig.FilterByAvailableHazardDose;
            _actionInputRequirements[ActionType.HazardCharacterisations].IsRequired = ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.HazardCharacterisations].IsVisible = ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionDataLinkRequirements[ScopingType.ActiveSubstancesModels][ScopingType.Compounds].AlertTypeMissingData = AlertType.None;
            _actionDataLinkRequirements[ScopingType.ActiveSubstancesModels][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ActiveSubstances][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override bool ShouldCompute {
            get {
                return CanCompute && _moduleSettings.IsCompute;
            }
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleAssessmentGroupMemberships) {
                result.Add(UncertaintySource.AssessmentGroupMemberships);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ActiveSubstancesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize();
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            // Get effects
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;

            // Get selected membership model
            var availableActiveSubstanceModels = subsetManager.AllActiveSubstances
                .Where(r => r.Effect != null || relevantEffects.Contains(r.Effect))
                .ToList();

            if (!availableActiveSubstanceModels?.Any() ?? true) {
                throw new Exception("No assessment group memberships model found for current effect selection");
            } else if (availableActiveSubstanceModels.Count > 1) {
                throw new Exception("Multiple assessment group memberships selected");
            }

            // Create aggregate memberships calculator and compute aggregate memberships
            // first: calculate the settings to use in the AggregateMembershipModelCalculator
            // TODO: refactor to clarify the intent
            var useProbabilisticMemberships = ModuleConfig.UseProbabilisticMemberships;
            var priorMembershipProbability = useProbabilisticMemberships
                ? ModuleConfig.PriorMembershipProbability
                : ModuleConfig.IncludeSubstancesWithUnknowMemberships ? 1 : 0;
            var includeSubstancesWithUnknownMemberships = useProbabilisticMemberships
                ? priorMembershipProbability > 0
                : ModuleConfig.IncludeSubstancesWithUnknowMemberships;
            var assessmentGroupCalculationMethod = useProbabilisticMemberships
                ? AssessmentGroupMembershipCalculationMethod.CrispMax
                : AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio;
            var combinationMethodMembershipInfoAndPodPresence = ModuleConfig.DeriveFromHazardData
                ? ModuleConfig.CombinationMethodMembershipInfoAndPodPresence
                : CombinationMethodMembershipInfoAndPodPresence.Union;

            var aggregateMembershipsCalculator = new AggregateMembershipModelCalculator(
                isProbabilistic: useProbabilisticMemberships,
                includeSubstancesWithUnknownMemberships: includeSubstancesWithUnknownMemberships,
                bubbleMembershipsThroughAop: ModuleConfig.BubbleMembershipsThroughAop,
                priorMembershipProbability: priorMembershipProbability,
                assessmentGroupMembershipCalculationMethod: assessmentGroupCalculationMethod,
                combinationMethodMembershipInfoAndPodPresence: combinationMethodMembershipInfoAndPodPresence
            );

            // Compute aggregate memberships
            var upstreamEffectsLookup = data.AdverseOutcomePathwayNetwork?.EffectRelations?
                .Where(r => relevantEffects.Contains(r.DownstreamKeyEvent) && relevantEffects.Contains(r.UpstreamKeyEvent))
                .ToLookup(r => r.DownstreamKeyEvent);
            var activeSubstanceModel = aggregateMembershipsCalculator.Compute(
                availableActiveSubstanceModels,
                data.AllCompounds,
                data.SelectedEffect,
                upstreamEffectsLookup
            );

            // If we also want to derive memberships from available pod, then update the memberships accordingly.
            if (ModuleConfig.DeriveFromHazardData) {
                var membershipsFromPoDCalculator = new MembershipsFromPodCalculator(
                    ModuleConfig.FilterByAvailableHazardCharacterisation,
                    ModuleConfig.FilterByAvailableHazardDose
                );
                var membershipsFromPoD = membershipsFromPoDCalculator.Compute(
                    data.SelectedEffect,
                    data.AllCompounds,
                    data.PointsOfDeparture,
                    data.HazardCharacterisationModelsCollections
                );
                activeSubstanceModel.MembershipProbabilities = aggregateMembershipsCalculator
                    .MergeMembershipsWithPodAvailability(data.AllCompounds, membershipsFromPoD, activeSubstanceModel.MembershipProbabilities);
                availableActiveSubstanceModels.Add(membershipsFromPoD);
            }

            //set active substance memberships to 0 based on configuration of excluded substances
            var excludeSubstanceCodes = ModuleConfig.ExcludeSelectedSubstances.ToHashSet();
            if (excludeSubstanceCodes.Count != 0) {
                foreach (var kvp in activeSubstanceModel.MembershipProbabilities
                    .Where(m => excludeSubstanceCodes.Contains(m.Key.Code))
                ) {
                    activeSubstanceModel.MembershipProbabilities[kvp.Key] = 0D;
                }
            }

            // Set active substances collection based on membership probabilities
            var activeSubstances = filterActiveSubstancesByMembership(
                data.AllCompounds,
                activeSubstanceModel.MembershipProbabilities,
                ModuleConfig.FilterByCertainAssessmentGroupMembership
            );

            // Update data
            data.ModuleOutputData[ActionType.ActiveSubstances] = new ActiveSubstancesOutputData() {
                ActiveSubstances = activeSubstances,
                AvailableActiveSubstanceModels = availableActiveSubstanceModels,
                MembershipProbabilities = activeSubstanceModel.MembershipProbabilities
            };
        }

        protected override void loadDefaultData(ActionData data) {
            var defaultMemberships = data.AllCompounds?.ToDictionary(c => c, c => 1D);
            if (defaultMemberships != null) {
                data.ActiveSubstances = defaultMemberships.Keys.ToHashSet();
                data.MembershipProbabilities = defaultMemberships;
            }
        }

        protected override ActiveSubstancesActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            // Get relevant effects
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;

            // Compute available assessment group memberhip models
            var calculator = new MembershipsFromInSilicoCalculator(
                ModuleConfig.UseMolecularDockingModels,
                ModuleConfig.UseQsarModels
            );
            var availableActiveSubstanceModels = calculator.CalculateAvailableMembershipModels(
                data.MolecularDockingModels,
                data.QsarMembershipModels,
                data.AllCompounds,
                relevantEffects
            );

            // Create aggregate memberships calculator and compute aggregate memberships
            // first: calculate the settings to use in the AggregateMembershipModelCalculator
            // TODO: refactor to clarify the intent
            var useQsarOrMolDocking = ModuleConfig.UseQsarModels || ModuleConfig.UseMolecularDockingModels;
            var useProbabilisticMemberships = useQsarOrMolDocking && ModuleConfig.UseProbabilisticMemberships;
            var priorMembershipProbability = useProbabilisticMemberships
                ? ModuleConfig.PriorMembershipProbability
                : ModuleConfig.IncludeSubstancesWithUnknowMemberships ? 1 : 0;
            var includeSubstancesWithUnknownMemberships = useProbabilisticMemberships
                ? priorMembershipProbability > 0
                : useQsarOrMolDocking
                    ? ModuleConfig.IncludeSubstancesWithUnknowMemberships
                    : !ModuleConfig.DeriveFromHazardData;
            var assessmentGroupCalculationMethod = useQsarOrMolDocking
                ? ModuleConfig.AssessmentGroupMembershipCalculationMethod
                : useProbabilisticMemberships
                    ? AssessmentGroupMembershipCalculationMethod.CrispMax
                    : AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio;
            var combinationMethodMembershipInfoAndPodPresence = useQsarOrMolDocking && ModuleConfig.DeriveFromHazardData
                ? ModuleConfig.CombinationMethodMembershipInfoAndPodPresence
                : CombinationMethodMembershipInfoAndPodPresence.Union;

            var aggregateMembershipsCalculator = new AggregateMembershipModelCalculator(
                isProbabilistic: useProbabilisticMemberships,
                includeSubstancesWithUnknownMemberships: includeSubstancesWithUnknownMemberships,
                bubbleMembershipsThroughAop: ModuleConfig.BubbleMembershipsThroughAop,
                priorMembershipProbability: priorMembershipProbability,
                assessmentGroupMembershipCalculationMethod: assessmentGroupCalculationMethod,
                combinationMethodMembershipInfoAndPodPresence: combinationMethodMembershipInfoAndPodPresence
            );

            var upstreamEffectsLookup = data.AdverseOutcomePathwayNetwork?.EffectRelations?
                .Where(r => relevantEffects.Contains(r.DownstreamKeyEvent) && relevantEffects.Contains(r.UpstreamKeyEvent))
                .ToLookup(r => r.DownstreamKeyEvent);
            var activeSubstanceModel = aggregateMembershipsCalculator.Compute(
                availableActiveSubstanceModels,
                data.AllCompounds,
                data.SelectedEffect,
                upstreamEffectsLookup
            );

            // Drilldown of experimental bubble memberships calculation
            Dictionary<Effect, ActiveSubstanceModel> aopNetworkEffectsAssessmentGroupMembershipModels = null;
            if (ModuleConfig.BubbleMembershipsThroughAop) {
                aopNetworkEffectsAssessmentGroupMembershipModels = aggregateMembershipsCalculator.ComputeAllUpstreamEffectMembershipModels(
                    availableActiveSubstanceModels,
                    data.AllCompounds,
                    data.SelectedEffect,
                    upstreamEffectsLookup
                );
            }

            // If we also want to derive memberships from available pod, then update the memberships accordingly.
            if (ModuleConfig.FilterByAvailableHazardDose
                || ModuleConfig.FilterByAvailableHazardCharacterisation
            ) {
                var membershipsFromPoDCalculator = new MembershipsFromPodCalculator(
                    ModuleConfig.FilterByAvailableHazardCharacterisation,
                    ModuleConfig.FilterByAvailableHazardDose
                );
                var membershipsFromPoD = membershipsFromPoDCalculator.Compute(
                    data.SelectedEffect,
                    data.AllCompounds,
                    data.PointsOfDeparture,
                    data.HazardCharacterisationModelsCollections
                );
                activeSubstanceModel.MembershipProbabilities = aggregateMembershipsCalculator
                    .MergeMembershipsWithPodAvailability(
                        data.AllCompounds,
                        membershipsFromPoD,
                        activeSubstanceModel.MembershipProbabilities
                    );
                availableActiveSubstanceModels.Add(membershipsFromPoD);
            }
            //set active substance memberships to 0 based on configuration of excluded substances
            var excludeSubstanceCodes = ModuleConfig.ExcludeSelectedSubstances.ToHashSet();
            if (excludeSubstanceCodes.Count != 0) {
                foreach (var kvp in activeSubstanceModel.MembershipProbabilities
                    .Where(m => excludeSubstanceCodes.Contains(m.Key.Code))
                ) {
                    activeSubstanceModel.MembershipProbabilities[kvp.Key] = 0D;
                }
            }

            // Set active substances collection based on membership probabilities
            var activeSubstances = filterActiveSubstancesByMembership(
                data.AllCompounds,
                activeSubstanceModel.MembershipProbabilities,
                !useProbabilisticMemberships || ModuleConfig.FilterByCertainAssessmentGroupMembership
            );

            var result = new ActiveSubstancesActionResult() {
                ActiveSubstances = activeSubstances,
                ActiveSubstanceModel = activeSubstanceModel,
                AvailableActiveSubstanceModels = availableActiveSubstanceModels,
                AopNetworkEffectsActiveSubstanceModels = aopNetworkEffectsAssessmentGroupMembershipModels?.Values.Where(r => r != null).ToList(),
                MembershipProbabilities = activeSubstanceModel.MembershipProbabilities,
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(ActiveSubstancesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            if (data.AvailableActiveSubstanceModels != null) {
                var summarizer = new ActiveSubstancesSummarizer();
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, ActiveSubstancesActionResult result) {
            var outputData = data.GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType);
            outputData.ActiveSubstances = result.ActiveSubstances;
            outputData.AvailableActiveSubstanceModels = result.AvailableActiveSubstanceModels;
            outputData.MembershipProbabilities = result.MembershipProbabilities;
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.AssessmentGroupMemberships)) {
                var outputData = data.GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType);
                var source = data.MembershipProbabilities;
                Dictionary<Compound, double> newMembershipProbabilities = resampleMemberships(uncertaintySourceGenerators[UncertaintySource.AssessmentGroupMemberships], source);
                outputData.MembershipProbabilities = newMembershipProbabilities;
            }
            localProgress.Update(100);
        }

        protected override ActiveSubstancesActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new ActiveSubstancesActionResult();
            if (factorialSet.Contains(UncertaintySource.AssessmentGroupMemberships)) {
                var source = data.MembershipProbabilities;
                result.MembershipProbabilities = resampleMemberships(uncertaintySourceGenerators[UncertaintySource.AssessmentGroupMemberships], source);
            } else {
                result.MembershipProbabilities = data.MembershipProbabilities;
            }
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationDataUncertain(ActionData data, ActiveSubstancesActionResult result) {
            var outputData = data.GetOrCreateModuleOutputData<ActiveSubstancesOutputData>(ActionType);
            outputData.MembershipProbabilities = result.MembershipProbabilities;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, ActiveSubstancesActionResult result) {
            var rawDataConverter = new RawActiveSubstancesDataConverter();
            var code = (data.SelectedEffect != null)
                ? $"AG-{data.SelectedEffect.Code}"
                : "AG-Critical-Effect";
            var name = code;
            var description = (data.SelectedEffect != null)
                ? $"Assessment group membership model for effect {data.SelectedEffect?.Name} ({data.SelectedEffect?.Code}) generated by MCRA."
                : $"Assessment group membership model for critical effect generated by MCRA";

            var reference = $"MCRA project {_project.Name} (id: {_project.Id}).";
            var rawData = rawDataConverter.ToRaw(code, name, description, reference, data.SelectedEffect, data.ReferenceSubstance, data.MembershipProbabilities);
            rawDataWriter.Set(rawData);
        }

        private ICollection<Compound> filterActiveSubstancesByMembership(
            ICollection<Compound> substances,
            IDictionary<Compound, double> membershipProbabilities,
            bool restrictToCertainMembership
        ) {
            var predicate = restrictToCertainMembership
                ? membershipProbabilities.Where(m => m.Value == 1D)
                : membershipProbabilities.Where(m => m.Value > 0);

            return substances.Intersect(predicate.Select(m => m.Key)).ToHashSet();
        }

        private Dictionary<Compound, double> resampleMemberships(IRandom generator, IDictionary<Compound, double> source) {
            var newMembershipProbabilities = new Dictionary<Compound, double>();
            foreach (var nominal in source) {
                var newMembership = generator.NextDouble() < nominal.Value ? 1D : 0D;
                newMembershipProbabilities.Add(nominal.Key, newMembership);
            }
            return newMembershipProbabilities;
        }
    }
}
