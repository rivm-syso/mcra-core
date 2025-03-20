using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {

    [ActionType(ActionType.IndoorAirConcentrations)]
    public class IndoorAirConcentrationsActionCalculator : ActionCalculatorBase<IIndoorAirConcentrationsActionResult> {

        public IndoorAirConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.IndoorAirConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var airConcentrationUnit = AirConcentrationUnit.ugPerm3;

            var adjustedIndoorAirConcentrations = subsetManager.AllIndoorAirConcentrations
                .Select(r => {
                    //var alignmentFactor = r.AirConcentrationUnit
                    //    .GetConcentrationAlignmentFactor(airConcentrationUnit, r.Substance.MolecularMass);                    
                    var alignmentFactor = 1d;
                    var conc = r.Concentration * alignmentFactor;
                    return new IndoorAirConcentration {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Location = r.Location,
                        Concentration = conc,
                        AirConcentrationUnit = airConcentrationUnit
                    };
                })
                .ToList();

            data.IndoorAirConcentrations = adjustedIndoorAirConcentrations;
            data.AirConcentrationUnit = airConcentrationUnit;
        }

        protected override void summarizeActionResult(IIndoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing indoor air concentrations", 0);
            var summarizer = new IndoorAirConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
