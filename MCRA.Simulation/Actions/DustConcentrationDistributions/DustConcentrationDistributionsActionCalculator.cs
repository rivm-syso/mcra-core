﻿using MCRA.Data.Compiled.Objects;
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

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {

    [ActionType(ActionType.DustConcentrationDistributions)]
    public class DustConcentrationDistributionsActionCalculator(ProjectDto project) : ActionCalculatorBase<IDustConcentrationDistributionsActionResult>(project) {
        private DustConcentrationDistributionsModuleConfig ModuleConfig => (DustConcentrationDistributionsModuleConfig)_moduleSettings;
        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }
        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleDustConcentrations) {
                result.Add(UncertaintySource.DustConcentrations);
            }
            return result;
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var dustConcentrationUnit = ConcentrationUnit.ugPerg;

            var adjustedDustConcentrationDistributions = subsetManager.AllDustConcentrationDistributions
                .Select(r => {
                    var alignmentFactor = r.Unit
                        .GetConcentrationAlignmentFactor(dustConcentrationUnit, r.Substance.MolecularMass);
                    var conc = r.Concentration * alignmentFactor;
                    return new DustConcentrationDistribution {
                        idSample = r.idSample,
                        Substance = r.Substance,
                        Concentration = conc,
                        Unit = dustConcentrationUnit
                    };
                })
                .OrderBy(c => c.idSample)
                .ToList();

            data.DustConcentrationDistributions = adjustedDustConcentrationDistributions;
            data.DustConcentrationUnit = dustConcentrationUnit;
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
                    data.DustConcentrationDistributions = [.. data.DustConcentrationDistributions
                        .GroupBy(c => c.Substance)
                        .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.DustConcentrations])
                            .Select(r => new DustConcentrationDistribution() {
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
        protected override void summarizeActionResult(IDustConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust concentration distributions", 0);
            var summarizer = new DustConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
