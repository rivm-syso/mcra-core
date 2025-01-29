using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {

    [ActionType(ActionType.SoilConcentrationDistributions)]
    public class SoilConcentrationDistributionsActionCalculator : ActionCalculatorBase<ISoilConcentrationDistributionsActionResult> {

        public SoilConcentrationDistributionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SoilConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var soilConcentrationUnit = ConcentrationUnit.ugPerg;

            var adjustedSoilConcentrationDistributions = subsetManager.AllSoilConcentrationDistributions
                .Select(r => {
                    var alignmentFactor = r.ConcentrationUnit
                        .GetConcentrationAlignmentFactor(soilConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new SoilConcentrationDistribution {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc,
                        ConcentrationUnit = soilConcentrationUnit
                    };
                })
                .ToList();

            data.SoilConcentrationDistributions = adjustedSoilConcentrationDistributions;
            data.SoilConcentrationUnit = soilConcentrationUnit;
        }

        protected override void summarizeActionResult(ISoilConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil concentration distributions", 0);
            var summarizer = new SoilConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
