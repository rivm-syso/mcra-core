using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsCollectionsGeneration;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.KineticModels {

    [ActionType(ActionType.KineticModels)]
    public sealed class KineticModelsActionCalculator : ActionCalculatorBase<IKineticModelsActionResult> {
        private KineticModelsModuleConfig ModuleConfig => (KineticModelsModuleConfig)_moduleSettings;

        public KineticModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var showActiveSubstances = GetRawDataSources().Any()
                && ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.KineticAbsorptionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new KineticModelsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var allAbsorptionFactors = subsetManager.AllAbsorptionFactors?.ToList() ?? [];

            var substanceSpecificAbsorptionFactors = allAbsorptionFactors?
                .Where(r => substances.Contains(r.Substance))
                .ToList();

            var absorptionFactorSettings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(absorptionFactorSettings);
            var absorptionFactors = absorptionFactorsCollectionBuilder
                .Create(substances, substanceSpecificAbsorptionFactors);

            data.AbsorptionFactors = absorptionFactors;

            var simpleKineticConversionFactors = absorptionFactors
                    .Select((c, ix) => KineticConversionFactor.FromDefaultAbsorptionFactor(c.ExposureRoute, c.Substance, c.AbsorptionFactor))
                    .ToList();
            data.SimpleAbsorptionFactorModels = simpleKineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, false)
                )
                .ToList();
            localProgress.Update(100);
        }

        protected override void loadDefaultData(ActionData data) {
            var settings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(settings);
            var absorptionFactors = absorptionFactorsCollectionBuilder
                .Create(data.ActiveSubstances)
                .Select(r => new SimpleAbsorptionFactor() {
                    Substance = r.Substance,
                    ExposureRoute = r.ExposureRoute,
                    AbsorptionFactor = r.AbsorptionFactor
                })
                .ToList();
            data.AbsorptionFactors = absorptionFactors;
        }

        protected override void summarizeActionResult(
            IKineticModelsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new KineticModelsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
