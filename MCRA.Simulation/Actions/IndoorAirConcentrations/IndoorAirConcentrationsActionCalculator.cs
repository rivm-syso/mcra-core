using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {

    [ActionType(ActionType.IndoorAirConcentrations)]
    public class IndoorAirConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<IIndoorAirConcentrationsActionResult>(project) {

        private IndoorAirConcentrationsModuleConfig ModuleConfig => (IndoorAirConcentrationsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.IndoorAirConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleAirConcentrations) {
                result.Add(UncertaintySource.AirConcentrations);
            }
            return result;
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var airConcentrationUnit = AirConcentrationUnit.ugPerm3;

            var adjustedIndoorAirConcentrations = subsetManager.AllIndoorAirConcentrations
                .Select(r => {
                    var alignmentFactor = 1d;
                    var conc = r.Concentration * alignmentFactor;
                    return new IndoorAirConcentration {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Location = r.Location,
                        Concentration = conc,
                        Unit = airConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.IndoorAirConcentrations = adjustedIndoorAirConcentrations;
            data.IndoorAirConcentrationUnit = airConcentrationUnit;
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.ResampleAirConcentrations) {
                if (factorialSet.Contains(UncertaintySource.AirConcentrations)) {
                    localProgress.Update("Resampling indoor air concentrations");
                    data.IndoorAirConcentrations = [.. data.IndoorAirConcentrations
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.AirConcentrations])
                            .Select(r => new IndoorAirConcentration() {
                                Substance = r.Substance,
                                Location = r.Location,
                                Concentration = r.Concentration,
                                idSample = r.idSample,
                                Unit = r.Unit,
                            })
                        )];
                }
            }
            localProgress.Update(100);
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
