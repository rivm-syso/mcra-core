using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {

    [ActionType(ActionType.IntraSpeciesFactors)]
    public sealed class IntraSpeciesFactorsActionCalculator : ActionCalculatorBase<IIntraSpeciesFactorsActionResult> {

        public IntraSpeciesFactorsActionCalculator(ProjectDto project) : base(project) {
            var showActiveSubstances = _project.AssessmentSettings.MultipleSubstances
                && !_project.EffectSettings.RestrictToAvailableHazardCharacterisations;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.IntraSpeciesModelParameters][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.IntraSpeciesModelParameters][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (_project.UncertaintyAnalysisSettings.ReSampleIntraSpecies) {
                result.Add(UncertaintySource.IntraSpecies);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new IntraSpeciesFactorsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadDefaultData(ActionData data) {
            var intraSpeciesFactorModelBuilder = new IntraSpeciesFactorModelBuilder();
            data.IntraSpeciesFactors = null;
            data.IntraSpeciesFactorModels = intraSpeciesFactorModelBuilder.Create(
                data.AllEffects,
                data.ActiveSubstances,
                null,
                _project.EffectModelSettings.DefaultIntraSpeciesFactor
            );
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var intraSpeciesFactors = subsetManager.AllIntraSpeciesFactors;
            var intraSpeciesFactorModelBuilder = new IntraSpeciesFactorModelBuilder();
            var intraSpeciesFactorModels = intraSpeciesFactorModelBuilder.Create(
                data.AllEffects,
                data.ActiveSubstances,
                intraSpeciesFactors,
                _project.EffectModelSettings.DefaultIntraSpeciesFactor
            );
            data.IntraSpeciesFactors = intraSpeciesFactors;
            data.IntraSpeciesFactorModels = intraSpeciesFactorModels;
        }

        protected override void summarizeActionResult(IIntraSpeciesFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            if (data.IntraSpeciesFactors != null) {
                var summarizer = new IntraSpeciesFactorsSummarizer();
                summarizer.Summarize(_project, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            if (factorialSet.Contains(UncertaintySource.IntraSpecies)) {
                var intraSpeciesFactorCalculator = new IntraSpeciesFactorModelBuilder();
                data.IntraSpeciesFactorModels = intraSpeciesFactorCalculator.Resample(
                    data.IntraSpeciesFactorModels,
                    uncertaintySourceGenerators[UncertaintySource.IntraSpecies]
                );
            }
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, IIntraSpeciesFactorsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var subHeader = header.GetSubSectionHeader<ActionSummaryBase>();
        }
    }
}
