using DocumentFormat.OpenXml.Office2013.Excel;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.Consumptions {

    [ActionType(ActionType.Consumptions)]
    public sealed class ConsumptionsActionCalculator : ActionCalculatorBase<IConsumptionsActionResult> {
        private ConsumptionsModuleConfig ModuleConfig => (ConsumptionsModuleConfig)_moduleSettings;

        public ConsumptionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            if (!IsLoopScope(ScopingType.FoodSurveys) && !IsLoopScope(ScopingType.Populations)) {
                _actionDataSelectionRequirements[ScopingType.FoodSurveys].MaxSelectionCount = 1;
            }
            _actionDataSelectionRequirements[ScopingType.FoodSurveys].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.DietaryIndividuals].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.DietaryIndividualDays].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.DietaryIndividualProperties].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.FoodSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.Consumptions][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.Consumptions][ScopingType.DietaryIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.DietaryIndividualDays][ScopingType.DietaryIndividuals].AlertTypeMissingData = AlertType.Notification;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ConsumptionsSettingsManager();
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleIndividuals) {
                result.Add(UncertaintySource.Individuals);
            }
            return result;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (IsLoopScope(ScopingType.FoodSurveys) || IsLoopScope(ScopingType.Populations)) {
                return true;
            }
            return linkManager.GetCodesInScope(ScopingType.FoodSurveys).Count == 1;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumptionSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            if (subsetManager.AllFoodSurveys == null || !subsetManager.AllFoodSurveys.Any()) {
                throw new Exception("No food consumption survey selected");
            }
            var selectedFoodSurveys = subsetManager.AllFoodSurveys.Values.ToList();
            if (!string.IsNullOrEmpty(data.SelectedPopulation.Location)) {
                selectedFoodSurveys = selectedFoodSurveys
                    .Where(c => string.IsNullOrEmpty(c.Location) || c.Location.Equals(data.SelectedPopulation.Location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!selectedFoodSurveys.Any()) {
                throw new Exception("No food consumption survey matches the specified population");
            } else if (selectedFoodSurveys.Count > 1) {
                throw new Exception("Multiple food consumption surveys selected");
            }

            var selectedFoodSurvey = selectedFoodSurveys.First();
            var selectedFoodConsumptions = subsetManager.AllFoodConsumptions.ToList();


            //TODO use IndividualSubsetCalculator class instead
            var individuals = IndividualsSubsetCalculator.GetIndividualSubsets(
                subsetManager.AllIndividuals.Values,
                subsetManager.AllIndividualProperties,
                data.SelectedPopulation,
                selectedFoodSurvey.Code,
                ModuleConfig.MatchIndividualSubsetWithPopulation,
                ModuleConfig.SelectedFoodSurveySubsetProperties,
                !ModuleConfig.IsDefaultSamplingWeight
            );

            // Filter individuals with < n survey days
            if (ModuleConfig.ExposureType == ExposureType.Chronic && ModuleConfig.ExcludeIndividualsWithLessThanNDays) {
                var maximumNumberOfDaysInSurvey = individuals.Max(c => c.NumberOfDaysInSurvey);
                if (ModuleConfig.MinimumNumberOfDays > maximumNumberOfDaysInSurvey) {
                    throw new Exception($"The specified number of nominal survey days for exclusion of individuals is {ModuleConfig.MinimumNumberOfDays}. Specify a value lower or equal to {maximumNumberOfDaysInSurvey}. ");
                }
                individuals = individuals.Where(c => c.NumberOfDaysInSurvey >= ModuleConfig.MinimumNumberOfDays).ToList();
            }

            // Fill individual cofactor and covariable fields
            IndividualsSubsetCalculator.FillIndividualCofactorCovariableValues(
                subsetManager.AllIndividualProperties,
                ModuleConfig.NameCofactor,
                ModuleConfig.NameCovariable,
                individuals
            );

            // Get the individual days
            var individualDays = individuals.SelectMany(r => r.IndividualDays.Values).ToHashSet();
            if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                individualDays = IndividualDaysGenerator.IncludeEmptyIndividualDays(individualDays, individuals);
            }

            // Create individual (subset) filters
            var individualDaysSubsetFiltersBuilder = new IndividualDaysSubsetFiltersBuilder();
            var individualDayFilters = individualDaysSubsetFiltersBuilder.Create(
                data.SelectedPopulation,
                ModuleConfig.MatchIndividualSubsetWithPopulation,
                ModuleConfig.SelectedFoodSurveySubsetProperties,
                true
            );

            // Get the individuals days subset and update the individuals
            if (individualDayFilters?.Count > 0) {
                foreach (var filter in individualDayFilters) {
                    individualDays = individualDays.Where(r => filter.Passes(r)).ToHashSet();
                    individuals = individualDays.Select(r => r.Individual).ToHashSet();
                }
            }

            // Get the food consumptions from the selected individuals
            selectedFoodConsumptions = selectedFoodConsumptions?
               .Where(a => individualDays.Contains(a.IndividualDay))
               .ToList() ?? [];

            // Restrict food consumptions based on the food as eaten subset
            if (ModuleConfig.RestrictConsumptionsByFoodAsEatenSubset && ModuleConfig.FoodAsEatenSubset.Any()) {
                var foodAsEatenSubsetCodes = ModuleConfig.FoodAsEatenSubset.ToHashSet(StringComparer.OrdinalIgnoreCase);
                selectedFoodConsumptions = selectedFoodConsumptions.Where(r => foodAsEatenSubsetCodes.Contains(r.Food.Code)).ToList();
            }

            // Restrict population by consumption
            if (ModuleConfig.ConsumerDaysOnly) {
                List<FoodConsumption> focalFoodConsumptions;
                if (ModuleConfig.RestrictPopulationByFoodAsEatenSubset && ModuleConfig.FocalFoodAsEatenSubset.Any()) {
                    // Get the focal food consumptions for the subset selection
                    var foodAsEatenSubsetCodes = ModuleConfig.FocalFoodAsEatenSubset.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var focalFoodsAsEaten = ModuleConfig.FocalFoodAsEatenSubset
                            .Where(r => data.AllFoodsByCode.ContainsKey(r))
                            .Select(r => data.AllFoodsByCode[r])
                            .ToHashSet();
                    focalFoodConsumptions = selectedFoodConsumptions
                        .Where(r => foodAsEatenSubsetCodes.Contains(r.Food.Code))
                        .ToList();
                } else {
                    focalFoodConsumptions = selectedFoodConsumptions;
                }

                // Filter the individuals/individual days by focal food consumption
                if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                    individuals = focalFoodConsumptions.Select(r => r.Individual).ToHashSet();
                    individualDays = individualDays.Where(r => individuals.Contains(r.Individual)).ToHashSet();
                } else {
                    individualDays = focalFoodConsumptions.Select(r => r.IndividualDay).ToHashSet();
                    individuals = individualDays.Select(r => r.Individual).ToHashSet();
                }

                // Filter the consumptions by selected individual days
                selectedFoodConsumptions = selectedFoodConsumptions
                    .Where(r => individualDays.Contains(r.IndividualDay))
                    .ToList();
            }

            // Get the consumed foods from the selected consumptions
            var foodsAsEaten = selectedFoodConsumptions?.Select(r => r.Food).ToHashSet();
            data.Cofactor = subsetManager.CofactorIndividualProperty;
            data.Covariable = subsetManager.CovariableIndividualProperty;
            data.FoodsAsEaten = foodsAsEaten;
            data.FoodSurvey = selectedFoodSurvey;
            data.ConsumerIndividuals = individuals;
            data.ConsumerIndividualDays = individualDays;
            data.SelectedFoodConsumptions = selectedFoodConsumptions;
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Bootstrap individuals
            if (factorialSet.Contains(UncertaintySource.Individuals)) {
                if (ModuleConfig.ExposureType == ExposureType.Acute) {
                    localProgress.Update("Resampling individual days");
                    data.ConsumerIndividualDays = data.ConsumerIndividualDays
                        .Resample(uncertaintySourceGenerators[UncertaintySource.Individuals])
                        .ToList();
                    data.ConsumerIndividuals = data.ConsumerIndividualDays
                        .Select(r => r.Individual)
                        .ToList();
                } else {
                    localProgress.Update("Resampling individuals");
                    data.ConsumerIndividuals = data.ConsumerIndividuals
                        .Resample(uncertaintySourceGenerators[UncertaintySource.Individuals])
                        .ToList();
                    data.ConsumerIndividualDays = data.ConsumerIndividualDays
                        .Where(r => data.ConsumerIndividuals.Contains(r.Individual))
                        .ToHashSet();
                }
            }

            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IConsumptionsActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConsumptionsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, result, data, header, order);
            localProgress.Update(100);
        }
    }
}
