using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    [ActionType(ActionType.HumanMonitoringData)]
    public class HumanMonitoringDataActionCalculator : ActionCalculatorBase<IHumanMonitoringDataActionResult> {

        public HumanMonitoringDataActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualPropertyValues].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringIndividualPropertyValues][ScopingType.HumanMonitoringIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleAnalyses][ScopingType.HumanMonitoringSamples].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.HumanMonitoringSampleAnalyses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringAnalyticalMethodCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HumanMonitoringDataSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var settings = new HumanMonitoringDataModuleSettings(_project);
            var surveys = subsetManager.AllHumanMonitoringSurveys;

            if (settings.SurveyCodes.Any()) {
                var selectedSurveyCodes = settings.SurveyCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
                surveys = surveys.Where(r => selectedSurveyCodes.Contains(r.Code)).ToList();
            }
            var samplingMethods = subsetManager.GetAllHumanMonitoringSamplingMethods();
            if (settings.SamplingMethodCodes?.Any() ?? false) {
                var selectedSamplingMethodCodes = settings.SamplingMethodCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
                samplingMethods = samplingMethods.Where(r => selectedSamplingMethodCodes.Contains(r.Code)).ToList();
            }
            if (!samplingMethods.Any()) {
                throw new Exception("Specified sampling method not found!");
            }

            // TODO: select only one Biological Matrix (scoping issue)
            samplingMethods = samplingMethods.Take(1).ToList();

            // Get individuals
            var surveyCodes = surveys.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var availableIndividuals = subsetManager
                .AllHumanMonitoringIndividuals
                .Where(r => surveyCodes.Contains(r.CodeFoodSurvey))
                .ToList();

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var individualFilters = individualsSubsetCalculator.Create(
                data.SelectedPopulation,
                subsetManager.AllHumanMonitoringIndividualProperties,
                settings.MatchHbmIndividualSubsetWithPopulation,
                settings.SelectedHbmSurveySubsetProperties
            );

            // Get the individuals from individual subset
            var individuals = IndividualsSubsetCalculator
                .ComputeIndividualsSubset(
                    availableIndividuals,
                    individualFilters
                );

            // Overwrite sampling weight
            if (!settings.UseHbmSamplingWeights) {
                foreach (var individual in individuals) {
                    individual.SamplingWeight = 1D;
                }
            }

            // Get the HBM samples
            var samples = subsetManager.AllHumanMonitoringSamples
                .Where(r => individuals.Contains(r.Individual))
                .Where(r => r.SamplingMethod == samplingMethods.FirstOrDefault())
                .ToList();

            // Get the concentration unit
            var concentrationUnit = settings.DefaultConcentrationUnit;
            var analyticalMethods = samples
                .SelectMany(s => s.SampleAnalyses.Select(sa => sa.AnalyticalMethod))
                .Distinct()
                .ToList();
            if (analyticalMethods.Any()) {
                var refAmc = (data.ReferenceCompound != null)
                    ? analyticalMethods
                        .Select(am => am.AnalyticalMethodCompounds.TryGetValue(data.ReferenceCompound, out var amc) ? amc : null)
                        .FirstOrDefault()
                    : analyticalMethods
                        .SelectMany(r => r.AnalyticalMethodCompounds.Values)
                        .FirstOrDefault();
                concentrationUnit = refAmc != null ? refAmc.GetConcentrationUnit() : concentrationUnit;
            }

            data.HbmSampleSubstanceCollections = HumanMonitoringSampleSubstanceCollectionsBuilder
                .Create(
                    data.AllCompounds,
                    samples,
                    concentrationUnit,
                    progressState
                );

            data.HbmSurveys = surveys;
            data.HbmIndividuals = individuals;
            data.HbmSamples = samples;
            data.HbmConcentrationUnit = concentrationUnit;
            data.HbmBiologicalMatrix = samplingMethods.First();
        }

        protected override void summarizeActionResult(IHumanMonitoringDataActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing human monitoring data", 0);
            var summarizer = new HumanMonitoringDataSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            // TODO
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, IHumanMonitoringDataActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var subHeader = header.GetSubSectionHeader<ActionSummaryBase>();
        }

        protected override void updateSimulationDataUncertain(ActionData data, IHumanMonitoringDataActionResult result) {
            updateSimulationData(data, result);
        }
    }
}