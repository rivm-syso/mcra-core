using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Data.Compiled.Objects;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ModelledFoods {


    [ActionType(ActionType.ModelledFoods)]
    public sealed class ModelledFoodsActionCalculator : ActionCalculatorBase<ModelledFoodsActionResult> {
        private ModelledFoodsModuleConfig ModuleConfig => (ModelledFoodsModuleConfig)_moduleSettings;

        public ModelledFoodsActionCalculator(ProjectDto project)
            : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = ModuleConfig.UseWorstCaseValues;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = ModuleConfig.UseWorstCaseValues;
            _actionInputRequirements[ActionType.Concentrations].IsVisible = ModuleConfig.DeriveModelledFoodsFromSampleBasedConcentrations;
            _actionInputRequirements[ActionType.Concentrations].IsRequired = ModuleConfig.DeriveModelledFoodsFromSampleBasedConcentrations;
            _actionInputRequirements[ActionType.SingleValueConcentrations].IsVisible = ModuleConfig.DeriveModelledFoodsFromSingleValueConcentrations;
            _actionInputRequirements[ActionType.SingleValueConcentrations].IsRequired = ModuleConfig.DeriveModelledFoodsFromSingleValueConcentrations;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ModelledFoodsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override ModelledFoodsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            // Compute substance sample statistics
            var settings = new ModelledFoodsInfosCalculatorSettings(ModuleConfig);
            var modelledFoodsInfosCalculator = new ModelledFoodsInfosCalculator(settings);

            var substances = new HashSet<Compound>();
            if (settings.DeriveModelledFoodsFromSampleBasedConcentrations) {
                substances = substances
                    .Union(data.ModelledSubstances)
                    .ToHashSet();
            }
            if (settings.DeriveModelledFoodsFromSingleValueConcentrations) {
                substances = substances
                    .Union(data.ActiveSubstanceSingleValueConcentrations.Keys.Select(r => r.Substance))
                    .ToHashSet();
            }
            if (settings.UseWorstCaseValues) {
                substances = substances
                    .Union(data.MaximumConcentrationLimits.Keys.Select(r => r.Substance))
                    .ToHashSet();
            }
            var substanceSampleStatistics = modelledFoodsInfosCalculator
                .Compute(
                    data.AllFoods,
                    substances,
                    settings.DeriveModelledFoodsFromSampleBasedConcentrations 
                        ? data.ActiveSubstanceSampleCollections : null,
                    settings.DeriveModelledFoodsFromSingleValueConcentrations 
                        ? data.ActiveSubstanceSingleValueConcentrations : null,
                    settings.UseWorstCaseValues 
                        ? data.MaximumConcentrationLimits : null
                );
            var modelledFoods = substanceSampleStatistics
                .Select(r => r.Food)
                .OrderBy(r => r.Code)
                .ToHashSet();

            var result = new ModelledFoodsActionResult() {
                ModelledFoods = modelledFoods,
                ModelledFoodsInfos = substanceSampleStatistics
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(ModelledFoodsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progress) {
            var localProgress = progress.NewProgressState(100);
            var summarizer = new ModelledFoodsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, ModelledFoodsActionResult actionResult) {
            data.ModelledFoods = actionResult.ModelledFoods;
            data.ModelledFoodInfos = actionResult.ModelledFoodsInfos.ToLookup(r => r.Food);
        }
    }
}
