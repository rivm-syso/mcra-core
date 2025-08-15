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

namespace MCRA.Simulation.Actions.OutdoorAirConcentrations {

    [ActionType(ActionType.OutdoorAirConcentrations)]
    public class OutdoorAirConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<IOutdoorAirConcentrationsActionResult>(project) {
        private OutdoorAirConcentrationsModuleConfig ModuleConfig => (OutdoorAirConcentrationsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.OutdoorAirConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
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
        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.ResampleAirConcentrations) {
                if (factorialSet.Contains(UncertaintySource.AirConcentrations)) {
                    localProgress.Update("Resampling outdoor air concentrations");
                    data.OutdoorAirConcentrations = [.. data.OutdoorAirConcentrations
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.AirConcentrations])
                            .Select(r => new OutdoorAirConcentration() {
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
        protected override void summarizeActionResult(IOutdoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing outdoor air concentrations", 0);
            var summarizer = new OutdoorAirConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
