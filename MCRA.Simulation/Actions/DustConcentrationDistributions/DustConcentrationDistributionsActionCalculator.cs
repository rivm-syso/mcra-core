using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.DustConcentrationModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {

    [ActionType(ActionType.DustConcentrationDistributions)]
    public class DustConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<DustConcentrationDistributionsActionResult>(project) {
        private DustConcentrationDistributionsModuleConfig ModuleConfig => (DustConcentrationDistributionsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var dustConcentrationUnit = ConcentrationUnit.ugPerg;

            var adjustedDustConcentrationDistributions = subsetManager.AllDustConcentrationDistributions
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(dustConcentrationUnit, r.Substance.MolecularMass);
                    var mean = r.Mean * alignmentFactor;
                    return new DustConcentrationDistribution {
                        IdDistribution = r.IdDistribution,
                        Substance = r.Substance,
                        Mean = mean,
                        Unit = dustConcentrationUnit,
                        DistributionType = r.DistributionType,
                        CvVariability = r.CvVariability,
                        OccurrencePercentage = r.OccurrencePercentage,
                    };
                })
                .OrderBy(c => c.IdDistribution)
                .ToList();

            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                adjustedDustConcentrationDistributions,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );

            data.DustConcentrationDistributions = adjustedDustConcentrationDistributions;
            data.DustConcentrationUnit = dustConcentrationUnit;
            data.DustConcentrationModels = concentrationModels;
        }

        protected override DustConcentrationDistributionsActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var concentrationModelsBuilder = new DustConcentrationModelBuilder();
            var concentrationModels = concentrationModelsBuilder.Create(
                data.DustConcentrations,
                NonDetectsHandlingMethod.ReplaceByZero,
                0
            );
            var result = new DustConcentrationDistributionsActionResult() {
                DustConcentrationModels = concentrationModels,
            };
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, DustConcentrationDistributionsActionResult result) {
            data.DustConcentrationModels = result.DustConcentrationModels;
        }

        protected override void summarizeActionResult(
            DustConcentrationDistributionsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust concentration distributions", 0);
            var summarizer = new DustConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
