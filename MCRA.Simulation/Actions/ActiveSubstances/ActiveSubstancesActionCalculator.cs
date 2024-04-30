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
                return CanCompute && _isCompute;
            }
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ReSampleAssessmentGroupMemberships) {
                result.Add(UncertaintySource.AssessmentGroupMemberships);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ActiveSubstancesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var settings = new ActiveSubstancesModuleSettings(ModuleConfig, false);

            // Get effects
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;

            // Get selected membership model
            var availableActiveSubstanceModels = subsetManager.AllActiveSubstances
                .Where(r => r.Effect != null || relevantEffects.Contains(r.Effect)).ToList();
            if (!availableActiveSubstanceModels?.Any() ?? true) {
                throw new Exception("No assessment group memberships model found for current effect selection");
            } else if (availableActiveSubstanceModels.Count > 1) {
                throw new Exception("Multiple assessment group memberships selected");
            }

            // Create aggregate memberships calculator
            var aggregateMembershipsCalculator = new AggregateMembershipModelCalculator(settings);

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
            if (settings.DeriveFromHazardData) {
                var membershipsFromPoDCalculator = new MembershipsFromPodCalculator(settings);
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

            // Update data
            data.ModuleOutputData[ActionType.ActiveSubstances] = new ActiveSubstancesOutputData() {
                ActiveSubstances = filterActiveSubstancesByMembership(
                    data.AllCompounds,
                    activeSubstanceModel.MembershipProbabilities,
                    settings.FilterByCertainAssessmentGroupMembership
                ),
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

            // Create action calculation settings from project
            var settings = new ActiveSubstancesModuleSettings(ModuleConfig, true);

            // Get relevant effects
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;

            // Compute available assessment group memberhip models
            var calculator = new MembershipsFromInSilicoCalculator(settings);
            var availableActiveSubstanceModels = calculator.CalculateAvailableMembershipModels(
                data.MolecularDockingModels,
                data.QsarMembershipModels,
                data.AllCompounds,
                relevantEffects
            );

            // Create aggregate memberships calculator and compute aggregate memberships
            var aggregateMembershipsCalculator = new AggregateMembershipModelCalculator(settings);
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
            if (settings.BubbleMembershipsThroughAop) {
                aopNetworkEffectsAssessmentGroupMembershipModels = aggregateMembershipsCalculator.ComputeAllUpstreamEffectMembershipModels(
                    availableActiveSubstanceModels,
                    data.AllCompounds,
                    data.SelectedEffect,
                    upstreamEffectsLookup
                );
            }

            // If we also want to derive memberships from available pod, then update the memberships accordingly.
            if (settings.RestrictToAvailableHazardDoses
                || settings.RestrictToAvailableHazardCharacterisations
            ) {
                var membershipsFromPoDCalculator = new MembershipsFromPodCalculator(settings);
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

            // Set active substances collection based on membership probabilities
            var activeSubstances = filterActiveSubstancesByMembership(
                data.AllCompounds,
                activeSubstanceModel.MembershipProbabilities,
                settings.FilterByCertainAssessmentGroupMembership
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
            if (restrictToCertainMembership) {
                var result = substances.Where(r => membershipProbabilities.TryGetValue(r, out var p) && p == 1D);
                return result.ToHashSet();
            } else {
                var result = substances.Where(r => membershipProbabilities.TryGetValue(r, out var p) && p > 0);
                return result.ToHashSet();
            }
        }

        private static Dictionary<Compound, double> resampleMemberships(IRandom generator, IDictionary<Compound, double> source) {
            var newMembershipProbabilities = new Dictionary<Compound, double>();
            foreach (var nominal in source) {
                var newMembership = generator.NextDouble() < nominal.Value ? 1 : 0;
                newMembershipProbabilities.Add(nominal.Key, newMembership);
            }
            return newMembershipProbabilities;
        }
    }
}
