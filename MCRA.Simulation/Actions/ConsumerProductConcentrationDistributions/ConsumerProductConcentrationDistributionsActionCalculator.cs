using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
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
            var consumerProductConcentrationUnit = ConcentrationUnit.ugPerKg;

            var adjustedConsumerProductConcentrationDistributions = subsetManager.AllConsumerProductConcentrationDistributions
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(consumerProductConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Mean * alignmentFactor;
                    return new ConsumerProductConcentrationDistribution {
                        Product = r.Product,
                        Substance = r.Substance,
                        Mean = conc,
                        Unit = consumerProductConcentrationUnit,
                        DistributionType = r.DistributionType,
                        CvVariability = r.CvVariability,
                        OccurrencePercentage = r.OccurrencePercentage,
                    };
                })
                .OrderBy(c => c.Product.Code)
                .ToList();
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                adjustedConsumerProductConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );

            data.AllConsumerProductConcentrationDistributions = adjustedConsumerProductConcentrationDistributions;
            data.ConsumerProductConcentrationUnit = consumerProductConcentrationUnit;
            data.ConsumerProductConcentrationModels = concentrationModels;
        }

        protected override ConsumerProductConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new ConsumerProductConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                data.AllConsumerProductConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new ConsumerProductConcentrationDistributionsActionResult() {
                ConsumerProductConcentrationModels = concentrationModels,
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
            var concentrationModels = concentrationModelsBuilder.Create(
                data.AllConsumerProductConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new ConsumerProductConcentrationDistributionsActionResult() {
                ConsumerProductConcentrationModels = concentrationModels,
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
