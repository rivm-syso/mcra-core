using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Action {

    public sealed class ActionSettingsSummarizer {

        public ActionSettingsSummary Summarize(
            ProjectDto project,
            ActionMapping actionMapping
        ) {
            var moduleSettingsSections = actionMapping
                .GetModuleMappings()
                .OrderByDescending(r => r.IsMainModule)
                .ThenByDescending(r => r.ModuleDefinition.ModuleType == ModuleType.PrimaryEntityModule)
                .ThenByDescending(r => r.Order)
                .Where(r => r.IsVisible)
                .Select(r => r.Settings)
                .ToList();
            var module = McraModuleDefinitions.Instance.ModuleDefinitions[actionMapping.MainActionType];
            var section = new ActionSettingsSummary("Action inputs");
            section.SubSections.AddRange(moduleSettingsSections);
            if (actionMapping.ModuleMappingsDictionary[actionMapping.MainActionType].IsCompute) {
                section.SubSections.Add(SummarizeRunSettings(project));
            }
            if (module.HasUncertaintyAnalysis) {
                section.SubSections.Add(SummarizeUncertainty(project, actionMapping));
            }
            if (actionMapping.OutputSettings.Any()) {
                section.SubSections.Add(SummarizeOutput(project, actionMapping));
            }

            var moduleMappings = actionMapping
                .GetModuleMappings()
                .OrderByDescending(r => r.IsMainModule)
                .ThenByDescending(r => r.ModuleDefinition.ModuleType == ModuleType.PrimaryEntityModule)
                .ThenByDescending(r => r.Order)
                .Where(r => r.IsVisible && !r.IsCompute)
                .ToList();

            return section;
        }

        public ActionSettingsSummary SummarizeDataSources(
            IEnumerable<IRawDataSourceVersion> dataSourceVersions,
            ActionMapping actionMapping
        ) {
            var moduleMappings = actionMapping
                .GetModuleMappings()
                .OrderByDescending(r => r.IsMainModule)
                .ThenByDescending(r => r.ModuleDefinition.ModuleType == ModuleType.PrimaryEntityModule)
                .ThenByDescending(r => r.Order)
                .Where(r => r.IsVisible && !r.IsCompute)
                .ToList();
            var section = new ActionSettingsSummary("Data sources");

            var rds = dataSourceVersions?
                .GroupBy(r => r.id)
                .ToDictionary(r => r.Key, r => r.First());

            var numberOfItems = Enum.GetNames(typeof(SourceTableGroup)).Length;
            foreach (var mapping in moduleMappings) {
                if (mapping.RawDataSources?.Any() ?? false) {
                    foreach (var mappingRds in mapping.RawDataSources) {
                        rds.TryGetValue(mappingRds, out var rdsItem);
                        section.SummarizeDataSource(mapping.TableGroup, rdsItem);
                    }
                }
            }
            return section;
        }

        public ActionSettingsSummary SummarizeOutput(ProjectDto project, ActionMapping actionMapping) {
            var section = new ActionSettingsSummary("Output settings");
            ActionModuleMapping mapping = null;
            if ((project.CalculationActionTypes.Contains(ActionType.DietaryExposures)
                || project.CalculationActionTypes.Contains(ActionType.TargetExposures)
                || project.CalculationActionTypes.Contains(ActionType.Risks)
                )
                || (actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.Risks, out mapping) && mapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.DietaryExposures, out mapping) && mapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.TargetExposures, out mapping) && mapping.IsCompute)
            ) {
                section.SummarizeSetting(SettingsItemType.SelectedPercentiles, project.OutputDetailSettings.SelectedPercentiles);
                section.SummarizeSetting(SettingsItemType.ExposureLevels, project.OutputDetailSettings.ExposureLevels);
                section.SummarizeSetting(SettingsItemType.ExposureMethod, project.OutputDetailSettings.ExposureMethod);
                section.SummarizeSetting(SettingsItemType.IsDetailedOutput, project.OutputDetailSettings.IsDetailedOutput);
                section.SummarizeSetting(SettingsItemType.PercentageForDrilldown, project.OutputDetailSettings.PercentageForDrilldown);

                if (project.IntakeModelSettings != null && project.IntakeModelSettings.CovariateModelling) {
                    section.SummarizeSetting(SettingsItemType.Intervals, project.OutputDetailSettings.Intervals);
                    section.SummarizeSetting(SettingsItemType.ExtraPredictionLevels, project.OutputDetailSettings.ExtraPredictionLevels);
                }

                section.SummarizeSetting(SettingsItemType.SummarizeSimulatedData, project.OutputDetailSettings.SummarizeSimulatedData);
            }

            section.SummarizeSetting(SettingsItemType.PercentageForUpperTail, project.OutputDetailSettings.PercentageForUpperTail);
            section.SummarizeSetting(SettingsItemType.LowerPercentage, project.OutputDetailSettings.LowerPercentage);
            section.SummarizeSetting(SettingsItemType.UpperPercentage, project.OutputDetailSettings.UpperPercentage);
            section.SummarizeSetting(SettingsItemType.IsPerPerson, project.SubsetSettings.IsPerPerson);
            return section;
        }

        public ActionSettingsSummary SummarizeRunSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Initialisation seed");
            section.SummarizeSetting(SettingsItemType.RandomSeed, project.MonteCarloSettings.RandomSeed);
            //var uss = project.UncertaintyAnalysisSettings;
            //section.SummarizeSetting(SettingsItemType.DoUncertaintyAnalysis, uss.DoUncertaintyAnalysis);
            //section.SummarizeSetting(SettingsItemType.DoUncertaintyFactorial, uss.DoUncertaintyFactorial);
            return section;
        }

        public ActionSettingsSummary SummarizeUncertainty(ProjectDto project, ActionMapping actionMapping) {
            var section = new ActionSettingsSummary("Uncertainty settings");
            var uss = project.UncertaintyAnalysisSettings;
            section.SummarizeSetting(SettingsItemType.DoUncertaintyAnalysis, uss.DoUncertaintyAnalysis);
            if (uss.DoUncertaintyAnalysis) {
                section.SummarizeSetting(SettingsItemType.NumberOfResampleCycles, uss.NumberOfResampleCycles);

                var activeUncertaintySettings = actionMapping.AvailableUncertaintySources;

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(SettingsItemType.NumberOfIterationsPerResampledSet, uss.NumberOfIterationsPerResampledSet);
                }

                if (actionMapping.ModuleDefinition.HasUncertaintyFactorial) {
                    section.SummarizeSetting(SettingsItemType.DoUncertaintyFactorial, uss.DoUncertaintyFactorial);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleConcentrations)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleConcentrations, uss.ReSampleConcentrations);
                    section.SummarizeSetting(SettingsItemType.IsParametric, uss.IsParametric);
                    if (activeUncertaintySettings.Contains(SettingsItemType.RecomputeOccurrencePatterns)) {
                        section.SummarizeSetting(SettingsItemType.RecomputeOccurrencePatterns, uss.RecomputeOccurrencePatterns);
                    }
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(SettingsItemType.ResampleIndividuals, uss.ResampleIndividuals);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleProcessingFactors)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleProcessingFactors, uss.ReSampleProcessingFactors);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleNonDietaryExposures)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleNonDietaryExposures, uss.ReSampleNonDietaryExposures);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleInterspecies)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleInterspecies, uss.ReSampleInterspecies);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleIntraSpecies)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleIntraSpecies, uss.ReSampleIntraSpecies);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleRPFs)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleRPFs, uss.ReSampleRPFs);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleParameterValues)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleParameterValues, uss.ReSampleParameterValues);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleAssessmentGroupMemberships)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleAssessmentGroupMemberships, uss.ReSampleAssessmentGroupMemberships);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleImputationExposureDistributions)) {
                    section.SummarizeSetting(SettingsItemType.ReSampleImputationExposureDistributions, uss.ReSampleImputationExposureDistributions);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleKineticModelParameters)) {
                    section.SummarizeSetting(SettingsItemType.ResampleKineticModelParameters, uss.ResampleKineticModelParameters);
                }
                section.SummarizeSetting(SettingsItemType.UncertaintyLowerBound, uss.UncertaintyLowerBound);
                section.SummarizeSetting(SettingsItemType.UncertaintyUpperBound, uss.UncertaintyUpperBound);
            }

            return section;
        }
    }
}
