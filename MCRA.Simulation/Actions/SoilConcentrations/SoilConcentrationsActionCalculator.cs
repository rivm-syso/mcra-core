using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.SoilConcentrations {

    [ActionType(ActionType.SoilConcentrations)]
    public class SoilConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<ISoilConcentrationsActionResult>(project) {
        private SoilConcentrationsModuleConfig ModuleConfig => (SoilConcentrationsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SoilConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }
        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleSoilConcentrations) {
                result.Add(UncertaintySource.SoilConcentrations);
            }
            return result;
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var adjustedSoilConcentrations = subsetManager.AllSoilConcentrations
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(SystemUnits.DefaultSoilConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new SubstanceConcentration {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc,
                        Unit = SystemUnits.DefaultSoilConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.SoilConcentrations = adjustedSoilConcentrations;
            data.SoilConcentrationUnit = SystemUnits.DefaultSoilConcentrationUnit;
        }
        protected override void loadDataUncertain(
           ActionData data,
           UncertaintyFactorialSet factorialSet,
           Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
           CompositeProgressState progressReport
       ) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.ResampleSoilConcentrations) {
                if (factorialSet.Contains(UncertaintySource.SoilConcentrations)) {
                    localProgress.Update("Resampling soil concentrations");
                    data.SoilConcentrations = [.. data.SoilConcentrations
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.SoilConcentrations])
                            .Select(r => new SubstanceConcentration() {
                                Substance = r.Substance,
                                Concentration = r.Concentration,
                                idSample = r.idSample,
                                Unit = r.Unit,
                            })
                        )];
                }
            }
            localProgress.Update(100);
        }
        protected override void summarizeActionResult(ISoilConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil concentrations", 0);
            var summarizer = new SoilConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
