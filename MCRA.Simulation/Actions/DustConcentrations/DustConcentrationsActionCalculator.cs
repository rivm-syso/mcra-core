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

namespace MCRA.Simulation.Actions.DustConcentrations {

    [ActionType(ActionType.DustConcentrations)]
    public class DustConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<IDustConcentrationsActionResult>(project) {
        private DustConcentrationsModuleConfig ModuleConfig => (DustConcentrationsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleDustConcentrations) {
                result.Add(UncertaintySource.DustConcentrations);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var adjustedDustConcentrations = subsetManager.AllDustConcentrations
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(SystemUnits.DefaultDustConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new DustConcentration {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc,
                        Unit = SystemUnits.DefaultDustConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.DustConcentrations = adjustedDustConcentrations;
            data.DustConcentrationUnit = SystemUnits.DefaultDustConcentrationUnit;
        }

        protected override void loadDataUncertain(
           ActionData data,
           UncertaintyFactorialSet factorialSet,
           Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
           CompositeProgressState progressReport
       ) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.ResampleDustConcentrations) {
                if (factorialSet.Contains(UncertaintySource.DustConcentrations)) {
                    localProgress.Update("Resampling dust concentrations");
                    data.DustConcentrations = [.. data.DustConcentrations
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.DustConcentrations])
                            .Select(r => new DustConcentration() {
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

        protected override void summarizeActionResult(IDustConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust concentrations", 0);
            var summarizer = new DustConcentrationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
