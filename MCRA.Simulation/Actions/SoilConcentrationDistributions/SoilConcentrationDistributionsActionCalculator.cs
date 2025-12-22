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

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {

    [ActionType(ActionType.SoilConcentrationDistributions)]
    public class SoilConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<ISoilConcentrationDistributionsActionResult>(project) {
        private SoilConcentrationDistributionsModuleConfig ModuleConfig => (SoilConcentrationDistributionsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SoilConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }
        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleSoilConcentrations) {
                result.Add(UncertaintySource.SoilConcentrations);
            }
            return result;
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var adjustedSoilConcentrationDistributions = subsetManager.AllSoilConcentrationDistributions
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(SystemUnits.DefaultSoilConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new SoilConcentrationDistribution {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc,
                        Unit = SystemUnits.DefaultSoilConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.SoilConcentrationDistributions = adjustedSoilConcentrationDistributions;
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
                    data.SoilConcentrationDistributions = [.. data.SoilConcentrationDistributions
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.SoilConcentrations])
                            .Select(r => new SoilConcentrationDistribution() {
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
        protected override void summarizeActionResult(ISoilConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil concentration distributions", 0);
            var summarizer = new SoilConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
