using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {

    [ActionType(ActionType.ConsumptionsByModelledFood)]
    public sealed class ConsumptionsByModelledFoodActionCalculator : ActionCalculatorBase<ConsumptionsByModelledFoodActionResult> {

        public ConsumptionsByModelledFoodActionCalculator(ProjectDto project)
            : base(project) {
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumptionsByModelledFoodSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override ConsumptionsByModelledFoodActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(50);
            localProgress.Update("Grouping consumptions by modelled food", 0);
            var modelledFoodConsumptions = data.SelectedFoodConsumptions;

            ICollection<Individual> modelledFoodConsumers;
            ICollection<IndividualDay> modelledFoodConsumerDays;
            if (_project.SubsetSettings.ModelledFoodsConsumerDaysOnly) {
                var filteredConversions = data.FoodConversionResults;

                // Filter conversions by focal modelled food subset
                if (_project.SubsetSettings.RestrictPopulationByModelledFoodSubset && _project.FocalFoodAsMeasuredSubset.Any()) {
                    var focalFoodsAsMeasuredCodes = _project.FocalFoodAsMeasuredSubset.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    filteredConversions = filteredConversions.Where(r => focalFoodsAsMeasuredCodes.Contains(r.FoodAsMeasured.Code)).ToList();
                }

                // Construct list of foods-as-eaten with potential intake (of selected focal foods)
                var potentialIntakeFocalFoodsAsEaten = filteredConversions
                    .Select(r => r.FoodAsEaten)
                    .ToHashSet();

                if (_project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                    // Filter individuals by focal foods as eaten
                    modelledFoodConsumers = data.SelectedFoodConsumptions
                        .Where(r => potentialIntakeFocalFoodsAsEaten.Contains(r.Food))
                        .Select(r => r.Individual)
                        .Distinct()
                        .ToList();
                    modelledFoodConsumerDays = data.ConsumerIndividualDays
                        .Where(r => modelledFoodConsumers.Contains(r.Individual))
                        .ToHashSet();
                } else {
                    modelledFoodConsumerDays = data.SelectedFoodConsumptions
                        .Where(r => potentialIntakeFocalFoodsAsEaten.Contains(r.Food))
                        .Select(r => r.IndividualDay)
                        .Distinct()
                        .ToList();
                    modelledFoodConsumers = modelledFoodConsumerDays
                        .Select(r => r.Individual)
                        .ToHashSet();
                }
                // Filter consumptions by selected individual days
                modelledFoodConsumptions = modelledFoodConsumptions
                    .Where(r => modelledFoodConsumerDays.Contains(r.IndividualDay))
                    .ToList();
            } else {
                // Include all individuals and all individual days
                modelledFoodConsumers = data.ConsumerIndividuals;
                modelledFoodConsumerDays = data.ConsumerIndividualDays;
            }

            // Filter consumptions by foods-as-eaten with potential intake
            var potentialIntakeFoodsAsEaten = data.FoodConversionResults.Select(r => r.FoodAsEaten).ToHashSet();
            modelledFoodConsumptions = modelledFoodConsumptions.Where(r => potentialIntakeFoodsAsEaten.Contains(r.Food)).ToList();

            // Compute consumptions by modelled food
            // TODO: for uncertainty calculations, recomputing consumptions by
            // modelled food is expensive and probably not (strickly) needed if
            // you only use this collection as a lookup; may be a candidate for
            // performance improvement.
            var consumptionsByModelledFoodCalculator = new ConsumptionsByModelledFoodCalculator();
            var consumptionsByModelledFood = consumptionsByModelledFoodCalculator
                .Compute(
                    data.FoodConversionResults,
                    modelledFoodConsumptions,
                    progressReport
                );

            var result = new ConsumptionsByModelledFoodActionResult() {
                ModelledFoodConsumers = modelledFoodConsumers,
                ModelledFoodConsumerDays = modelledFoodConsumerDays,
                ConsumptionsByModelledFood = consumptionsByModelledFood.OrderBy(c => c.FoodAsMeasured.Code, StringComparer.OrdinalIgnoreCase).ToList(),
                ConsumptionsFoodsAsEaten = modelledFoodConsumptions,
            };

            localProgress.Update(100);
            return result;
        }

        /// <summary>
        /// Individuals and food consumptions are updated/filtered based on potential intake and possible consumer-only subset selections
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        protected override void updateSimulationData(ActionData data, ConsumptionsByModelledFoodActionResult result) {
            data.ModelledFoodConsumers = result.ModelledFoodConsumers;
            data.ModelledFoodConsumerDays = result.ModelledFoodConsumerDays;
            data.ConsumptionsByModelledFood = result.ConsumptionsByModelledFood;
            data.SelectedFoodConsumptions = result.ConsumptionsFoodsAsEaten;
        }

        protected override ConsumptionsByModelledFoodActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            return run(data, progressReport);
        }

        protected override void summarizeActionResult(ConsumptionsByModelledFoodActionResult result, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new ConsumptionsByModelledFoodSummarizer(progressReport);
            summarizer.Summarize(_project, result, data, header, order);
        }
    }
}
