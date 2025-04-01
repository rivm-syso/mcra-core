using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.OutdoorAirConcentrations {

    [ActionType(ActionType.OutdoorAirConcentrations)]
    public class OutdoorAirConcentrationsActionCalculator : ActionCalculatorBase<IOutdoorAirConcentrationsActionResult> {

        public OutdoorAirConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.OutdoorAirConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var airConcentrationUnit = AirConcentrationUnit.ugPerm3;

            var adjustedOutdoorAirConcentrations = subsetManager.AllOutdoorAirConcentrations
                .Select(r => {
                    //var alignmentFactor = r.AirConcentrationUnit
                    //    .GetConcentrationAlignmentFactor(airConcentrationUnit, r.Substance.MolecularMass);
                    var alignmentFactor = 1d;
                    var conc = r.Concentration * alignmentFactor;
                    return new OutdoorAirConcentration {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Location = r.Location,
                        Concentration = conc,
                        Unit = airConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.OutdoorAirConcentrations = adjustedOutdoorAirConcentrations;
            data.IndoorAirConcentrationUnit = airConcentrationUnit;
        }

        protected override void summarizeActionResult(IOutdoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing outdoor air concentrations", 0);
            var summarizer = new OutdoorAirConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
