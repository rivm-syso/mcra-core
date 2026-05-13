using MCRA.Data.Compiled.Interfaces;
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
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.SubstanceConcentrations;
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
            var targetUnit = new StandardConcentrationUnitSpec(SystemUnits.DefaultSoilConcentrationUnit);
            var alignedConcentrations = subsetManager.AllSoilConcentrations
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(targetUnit.Unit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new AlignedSubstanceConcentration {
                        SampleId = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc
                    };
                })
                .OrderBy(c => c.SampleId)
                .ToList();

            data.SoilConcentrationsCollection = new SubstanceConcentrationsCollection(alignedConcentrations, targetUnit);
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
                    var unit = data.SoilConcentrationsCollection.Unit;
                    var resampled = data.SoilConcentrationsCollection
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.SoilConcentrations])
                            .Select(r => new SubstanceConcentration() {
                                idSample = (r as SubstanceConcentration).idSample,
                                Substance = r.Substance,
                                Concentration = r.Concentration,
                            })
                        )
                        .ToList();
                    data.SoilConcentrationsCollection = new(resampled, unit);
                }
            }
            localProgress.Update(100);
        }
        protected override void summarizeActionResult(
            ISoilConcentrationsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil concentrations", 0);
            var summarizer = new SoilConcentrationsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(
            UncertaintyFactorialSet factorialSet,
            ISoilConcentrationsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SoilConcentrationsSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(data, actionResult, header);
            localProgress.Update(100);
        }
    }
}
