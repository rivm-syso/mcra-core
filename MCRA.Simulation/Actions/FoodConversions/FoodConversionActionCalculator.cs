﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.FoodConversionCalculation;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.FoodConversions {

    [ActionType(ActionType.FoodConversions)]
    public sealed class FoodConversionsActionCalculator : ActionCalculatorBase<FoodConversionActionResult> {
        private FoodConversionsModuleConfig ModuleConfig => (FoodConversionsModuleConfig)_moduleSettings;

        public FoodConversionsActionCalculator(ProjectDto project)
            : base(project) {
        }

        protected override void verify() {
            var isTotalDietStudy = ModuleConfig.TotalDietStudy;
            var isProcessing = ModuleConfig.UseProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = !isTotalDietStudy && isProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = !isTotalDietStudy && isProcessing;
            _actionInputRequirements[ActionType.Consumptions].IsRequired = true;
            _actionInputRequirements[ActionType.Consumptions].IsVisible = true;
            _actionInputRequirements[ActionType.FoodRecipes].IsRequired = ModuleConfig.UseComposition;
            _actionInputRequirements[ActionType.FoodRecipes].IsVisible = ModuleConfig.UseComposition;
            _actionInputRequirements[ActionType.TotalDietStudyCompositions].IsRequired = isTotalDietStudy;
            _actionInputRequirements[ActionType.TotalDietStudyCompositions].IsVisible = isTotalDietStudy;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsRequired = ModuleConfig.UseReadAcrossFoodTranslations;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsVisible = ModuleConfig.UseReadAcrossFoodTranslations;
            _actionInputRequirements[ActionType.MarketShares].IsRequired = ModuleConfig.UseMarketShares;
            _actionInputRequirements[ActionType.MarketShares].IsVisible = ModuleConfig.UseMarketShares;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = false;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new FoodConversionSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize();
        }

        protected override FoodConversionActionResult run(ActionData data, CompositeProgressState progress) {
            var localProgress = progress.NewProgressState(20);

            var tdsFoodSampleCompositionDictionary = createTdsFoodSampleCompositionDictionary(data.TdsFoodCompositions);

            var samplesPerFoodSubstance = new Dictionary<(Food, Compound), ModelledFoodInfo>();
            foreach (var foodRecord in data.ModelledFoodInfos) {
                foreach (var substanceRecord in foodRecord.ToList()) {
                    samplesPerFoodSubstance.Add((foodRecord.Key, substanceRecord.Substance), substanceRecord);
                }
            }
            var conversionCalculator = new FoodConversionCalculator(
                ModuleConfig,
                data.AllFoodsByCode,
                samplesPerFoodSubstance,
                data.ModelledFoods,
                data.FoodExtrapolations,
                data.FoodRecipes?.ToLookup(r => r.FoodFrom),
                tdsFoodSampleCompositionDictionary,
                data.ProcessingTypes,
                data.MarketShares,
                data.ProcessingFactors
            );
            var conversionResults = conversionCalculator.Calculate(
                data.FoodsAsEaten,
                data.ActiveSubstances,
                progress.NewCompositeState(80)
            );

            var foodConversionResults = conversionResults.Where(c => c.ConversionStepResults.Last().Finished == true).ToList();
            var failedFoodConversionResults = conversionResults.Where(c => c.ConversionStepResults.Last().Finished == false).ToList();

            var result = new FoodConversionActionResult() {
                FoodConversionResults = foodConversionResults,
                FailedFoodConversionResults = failedFoodConversionResults
            };

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(FoodConversionActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progress) {
            var localProgress = progress.NewProgressState(100);
            localProgress.Update("Summarizing food conversion", 0);
            var summarizer = new FoodConversionsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, FoodConversionActionResult actionResult) {
            data.FoodConversionResults = actionResult.FoodConversionResults;
        }

        private static Dictionary<Food, Food> createTdsFoodSampleCompositionDictionary(ILookup<Food, TDSFoodSampleComposition> tdsCompositionLookup) {
            if (tdsCompositionLookup != null) {
                var result = new Dictionary<Food, Food>();
                foreach (var record in tdsCompositionLookup) {
                    foreach (var composition in record) {
                        result.Add(composition.Food, record.Key);
                    }
                }
                return result;
            }
            return null;
        }
    }
}
