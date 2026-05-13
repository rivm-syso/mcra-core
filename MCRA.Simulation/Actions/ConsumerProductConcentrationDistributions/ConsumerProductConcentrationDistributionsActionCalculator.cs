using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ConsumerProductConcentrationModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrationDistributions {

    [ActionType(ActionType.ConsumerProductConcentrationDistributions)]
    public sealed class ConsumerProductConcentrationDistributionsActionCalculator(ProjectDto project)
        : ActionCalculatorBase<ConsumerProductConcentrationDistributionsActionResult>(project) {
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrationDistributions][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var defaultConsumerProductConcentrationUnit = new ConsumerProductConcentrationUnitSpec(SystemUnits.DefaultConsumerProductConcentrationUnit);
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();

            var distributionsPerProduct = subsetManager.AllConsumerProductConcentrationDistributions
                .GroupBy(c => c.Product);
            var concentrationModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();
            foreach (var group in distributionsPerProduct) {
                var product = group.Key;
                var distributions = group.ToList();
                if (distributions.Count == 0) {
                    continue;
                }
                var models = concentrationModelsBuilder.CreateByDistributions(
                    distributions,
                    NonDetectsHandlingMethod.ReplaceByZero,
                    0,
                    defaultConsumerProductConcentrationUnit
                );
                foreach (var (compound, model) in models) {
                    concentrationModels[(product, compound)] = model;
                }
            }

            data.ConsumerProductConcentrationModels = concentrationModels;
        }

        protected override ConsumerProductConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();
            var allModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();
            foreach (var collection in data.AllConsumerProductConcentrationsCollections) {
                var models = concentrationModelsBuilder.CreateByConcentrations(
                    collection,
                    NonDetectsHandlingMethod.ReplaceByZero,
                    0,
                    collection.Unit
                );
                foreach (var (compound, model) in models) {
                    allModels[(collection.Product, compound)] = model;
                }
            }

            var result = new ConsumerProductConcentrationDistributionsActionResult() {
                ConsumerProductConcentrationModels = allModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override ConsumerProductConcentrationDistributionsActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();
            var allModels = new Dictionary<(ConsumerProduct, Compound), ConcentrationModel>();
            foreach (var collection in data.AllConsumerProductConcentrationsCollections) {
                var models = concentrationModelsBuilder.CreateByConcentrations(
                    collection,
                    NonDetectsHandlingMethod.ReplaceByZero,
                    0,
                    collection.Unit
                );
                foreach (var (compound, model) in models) {
                    allModels[(collection.Product, compound)] = model;
                }
            }
            var result = new ConsumerProductConcentrationDistributionsActionResult() {
                ConsumerProductConcentrationModels = allModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(ConsumerProductConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing consumer product concentration distributions", 0);
            var summarizer = new ConsumerProductConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationData(ActionData data, ConsumerProductConcentrationDistributionsActionResult result) {
            data.ConsumerProductConcentrationModels = result.ConsumerProductConcentrationModels;
        }

        protected override void updateSimulationDataUncertain(ActionData data, ConsumerProductConcentrationDistributionsActionResult result) {
            data.ConsumerProductConcentrationModels = result.ConsumerProductConcentrationModels;
        }
        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, ConsumerProductConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            //var summarizer = new ConcentrationModelsSummarizer();
            //summarizer.SummarizeUncertain(_actionSettings, actionResult, header);
            localProgress.Update(100);
        }
    }
}
