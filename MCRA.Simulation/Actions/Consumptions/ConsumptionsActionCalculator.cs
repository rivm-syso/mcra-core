using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Utils.Statistics;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Consumptions {

    [ActionType(ActionType.Consumptions)]
    public sealed class ConsumptionsActionCalculator : ActionCalculatorBase<IConsumptionsActionResult> {

        public ConsumptionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            if (_project.LoopScopingTypes == null 
                || (!_project.LoopScopingTypes.Contains(ScopingType.FoodSurveys) 
                    && !_project.LoopScopingTypes.Contains(ScopingType.Populations))
            ) {
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
            if (_project.UncertaintyAnalysisSettings.ResampleIndividuals) {
                result.Add(UncertaintySource.Individuals);
            }
            return result;
        }


        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (_project.LoopScopingTypes?.Contains(ScopingType.FoodSurveys) ?? false) {
                return true;
            } else if (_project.LoopScopingTypes?.Contains(ScopingType.Populations) ?? false) {
                return true;
            }
            return linkManager.GetCodesInScope(ScopingType.FoodSurveys).Count == 1;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumptionSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var settings = new ConsumptionsModuleSettings(_project);

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
            var availableIndividuals = subsetManager
                .AllIndividuals
                .Values
                .Where(c => selectedFoodSurvey.Code == c.CodeFoodSurvey)
                .ToList();

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var individualFilters = individualsSubsetCalculator.Create(
                data.SelectedPopulation,
                subsetManager.AllIndividualProperties,
                settings.MatchIndividualSubsetWithPopulation,
                settings.SelectedFoodSurveySubsetProperties
            );

            // Get the individuals from individual subset
            var individuals = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                availableIndividuals,
                individualFilters
            );

            // Filter individuals with < n survey days
            if (settings.ExposureType == ExposureType.Chronic && settings.ExcludeIndividualsWithLessThanNDays) {
                var maximumNumberOfDaysInSurvey = individuals.Max(c => c.NumberOfDaysInSurvey);
                if (settings.MinimumNumberOfDays > maximumNumberOfDaysInSurvey) {
                    throw new Exception($"The specified number of nominal survey days for exclusion of individuals is {settings.MinimumNumberOfDays}. Specify a value lower or equal to {maximumNumberOfDaysInSurvey}. ");
                }
                individuals = individuals.Where(c => c.NumberOfDaysInSurvey >= settings.MinimumNumberOfDays).ToList();
            }

            // Overwrite sampling weight
            if (settings.IsDefaultSamplingWeight) {
                foreach (var individual in individuals) {
                    individual.SamplingWeight = 1D;
                }
            }

            // Fill individual cofactor and covariable fields
            IndividualsSubsetCalculator.FillIndividualCofactorCovariableValues(
                subsetManager.AllIndividualProperties,
                settings.NameCofactor,
                settings.NameCovariable,
                individuals
            );

            // Get the individual days
            var individualDays = individuals.SelectMany(r => r.IndividualDays.Values).ToHashSet();
            if (settings.ExposureType == ExposureType.Chronic) {
                individualDays = IndividualDaysGenerator.IncludeEmptyIndividualDays(individualDays, individuals);
            }

            // Create individual (subset) filters
            var individualDaysSubsetFiltersBuilder = new IndividualDaysSubsetFiltersBuilder();
            var individualDayFilters = individualDaysSubsetFiltersBuilder.Create(
                data.SelectedPopulation,
                settings.MatchIndividualSubsetWithPopulation,
                settings.SelectedFoodSurveySubsetProperties,
                true
            );

            // Get the individuals days subset and update the individuals
            if (individualDayFilters?.Any() ?? false) {
                foreach (var filter in individualDayFilters) {
                    individualDays = individualDays.Where(r => filter.Passes(r)).ToHashSet();
                    individuals = individualDays.Select(r => r.Individual).ToHashSet();
                }
            }

            // Get the food consumptions from the selected individuals
            selectedFoodConsumptions = selectedFoodConsumptions?
               .Where(a => individualDays.Contains(a.IndividualDay))
               .ToList() ?? new List<FoodConsumption>();

            // Restrict food consumptions based on the food as eaten subset
            if (settings.RestrictConsumptionsByFoodAsEatenSubset && settings.FoodAsEatenSubset.Any()) {
                var foodAsEatenSubsetCodes = settings.FoodAsEatenSubset.Select(f => f.CodeFood).ToHashSet(StringComparer.OrdinalIgnoreCase);
                selectedFoodConsumptions = selectedFoodConsumptions.Where(r => foodAsEatenSubsetCodes.Contains(r.Food.Code)).ToList();
            }

            // Restrict population by consumption
            if (settings.ConsumerDaysOnly) {
                List<FoodConsumption> focalFoodConsumptions;
                if (settings.RestrictPopulationByFoodAsEatenSubset && settings.FocalFoodAsEatenSubset.Any()) {
                    // Get the focal food consumptions for the subset selection
                    var foodAsEatenSubsetCodes = settings.FocalFoodAsEatenSubset.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var focalFoodsAsEaten = settings.FocalFoodAsEatenSubset
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
                if (settings.ExposureType == ExposureType.Chronic) {
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

            var settings = new ConsumptionsModuleSettings(_project);

            // Bootstrap individuals
            if (factorialSet.Contains(UncertaintySource.Individuals)) {
                if (settings.ExposureType == ExposureType.Acute) {
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
            var summarizer = new ConsumptionsSummarizer();
            summarizer.Summarize(_project, result, data, header, order);
            localProgress.Update(100);
        }
    }
}
