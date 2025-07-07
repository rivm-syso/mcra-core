using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations {

    [ActionType(ActionType.ConsumerProductConcentrations)]
    public class ConsumerProductConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<IConsumerProductConcentrationsActionResult>(project) {
        private ConsumerProductConcentrationsModuleConfig ModuleConfig => (ConsumerProductConcentrationsModuleConfig)_moduleSettings;

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrations][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var consumerProductConcentrationUnit = ConcentrationUnit.ugPerKg;

            var adjustedConsumerProductConcentrations = subsetManager.AllConsumerProductConcentrations
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(consumerProductConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new ConsumerProductConcentration {
                        Product = r.Product,
                        Substance = r.Substance,
                        Concentration = conc,
                        Unit = consumerProductConcentrationUnit,
                        SamplingWeight = r.SamplingWeight,
                    };
                })
                .OrderBy(c => c.Product.Code)
                .ToList();

            data.AllConsumerProductConcentrations = adjustedConsumerProductConcentrations;
            data.ConsumerProductConcentrationUnit = consumerProductConcentrationUnit;
        }

        protected override void summarizeActionResult(IConsumerProductConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing consumer product concentration distributions", 0);
            var summarizer = new ConsumerProductConcentrationsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
