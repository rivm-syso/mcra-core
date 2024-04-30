using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.ModuleDefinitions.Settings;
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
            var actionConfig = project.GetModuleConfiguration<ActionModuleConfig>();
            var dietaryConfig = project.GetModuleConfiguration<DietaryExposuresModuleConfig>();

            ActionModuleMapping moduleMapping = null;
            if ((project.CalculationActionTypes.Contains(ActionType.DietaryExposures)
                || project.CalculationActionTypes.Contains(ActionType.TargetExposures)
                || project.CalculationActionTypes.Contains(ActionType.Risks)
                )
                || (actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.Risks, out moduleMapping) && moduleMapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.DietaryExposures, out moduleMapping) && moduleMapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.TargetExposures, out moduleMapping) && moduleMapping.IsCompute)
            ) {
                section.SummarizeSetting(SettingsItemType.SelectedPercentiles, dietaryConfig.SelectedPercentiles);
                section.SummarizeSetting(SettingsItemType.ExposureLevels, dietaryConfig.ExposureLevels);
                section.SummarizeSetting(SettingsItemType.ExposureMethod, dietaryConfig.ExposureMethod);

                if (dietaryConfig.CovariateModelling) {
                    section.SummarizeSetting(SettingsItemType.Intervals, dietaryConfig.Intervals);
                    section.SummarizeSetting(SettingsItemType.ExtraPredictionLevels, dietaryConfig.ExtraPredictionLevels);
                }
            }

            if (actionMapping.OutputSettings.Contains(SettingsItemType.SkipPrivacySensitiveOutputs)) {
                section.SummarizeSetting(SettingsItemType.SkipPrivacySensitiveOutputs, actionConfig.SkipPrivacySensitiveOutputs);
                if (actionMapping.OutputSettings.Contains(SettingsItemType.StoreIndividualDayIntakes)) {
                    section.SummarizeSetting(
                        SettingsItemType.StoreIndividualDayIntakes,
                        project.GetModuleConfiguration<TargetExposuresModuleConfig>().StoreIndividualDayIntakes
                    );
                }
                if (actionMapping.OutputSettings.Contains(SettingsItemType.IsDetailedOutput)) {
                    section.SummarizeSetting(SettingsItemType.IsDetailedOutput, dietaryConfig.IsDetailedOutput);
                    section.SummarizeSetting(SettingsItemType.PercentageForDrilldown, dietaryConfig.PercentageForDrilldown);
                }
            }

            section.SummarizeSetting(SettingsItemType.PercentageForUpperTail, actionConfig.PercentageForUpperTail);
            section.SummarizeSetting(SettingsItemType.LowerPercentage, actionConfig.LowerPercentage);
            section.SummarizeSetting(SettingsItemType.UpperPercentage, actionConfig.UpperPercentage);
            section.SummarizeSetting(SettingsItemType.IsPerPerson, dietaryConfig.IsPerPerson);
            return section;
        }

        public ActionSettingsSummary SummarizeRunSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Initialisation seed");
            section.SummarizeSetting(SettingsItemType.RandomSeed, project.GetModuleConfiguration<ActionModuleConfig>().RandomSeed);
            //var uss = project.UncertaintyAnalysisSettings;
            //section.SummarizeSetting(SettingsItemType.DoUncertaintyAnalysis, uss.DoUncertaintyAnalysis);
            //section.SummarizeSetting(SettingsItemType.DoUncertaintyFactorial, uss.DoUncertaintyFactorial);
            return section;
        }

        public ActionSettingsSummary SummarizeUncertainty(ProjectDto project, ActionMapping actionMapping) {
            var section = new ActionSettingsSummary("Uncertainty settings");
            var actionConfig = project.GetModuleConfiguration<ActionModuleConfig>();
            var dietaryConfig = project.GetModuleConfiguration<DietaryExposuresModuleConfig>();

            section.SummarizeSetting(SettingsItemType.DoUncertaintyAnalysis, actionConfig.DoUncertaintyAnalysis);
            if (actionConfig.DoUncertaintyAnalysis) {
                section.SummarizeSetting(SettingsItemType.NumberOfResampleCycles, actionConfig.NumberOfResampleCycles);

                var activeUncertaintySettings = actionMapping.AvailableUncertaintySources;

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.NumberOfIterationsPerResampledSet,
                        dietaryConfig.NumberOfIterationsPerResampledSet
                    );
                }

                if (actionMapping.ModuleDefinition.HasUncertaintyFactorial) {
                    section.SummarizeSetting(SettingsItemType.DoUncertaintyFactorial, actionConfig.DoUncertaintyFactorial);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleConcentrations)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleConcentrations,
                        project.GetModuleConfiguration<ConcentrationsModuleConfig>().ReSampleConcentrations
                    );
                    section.SummarizeSetting(
                        SettingsItemType.IsParametric,
                        project.GetModuleConfiguration<ConcentrationModelsModuleConfig>().IsParametric
                    );
                    if (activeUncertaintySettings.Contains(SettingsItemType.RecomputeOccurrencePatterns)) {
                        section.SummarizeSetting(
                            SettingsItemType.RecomputeOccurrencePatterns,
                            project.GetModuleConfiguration<OccurrencePatternsModuleConfig>().RecomputeOccurrencePatterns
                        );
                    }
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleIndividuals,
                        project.GetModuleConfiguration<ConsumptionsModuleConfig>().ResampleIndividuals
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleProcessingFactors)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleProcessingFactors,
                        project.GetModuleConfiguration<ProcessingFactorsModuleConfig>().ReSampleProcessingFactors
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleNonDietaryExposures)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleNonDietaryExposures,
                        project.GetModuleConfiguration<NonDietaryExposuresModuleConfig>().ReSampleNonDietaryExposures
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleInterspecies)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleInterspecies,
                        project.GetModuleConfiguration<InterSpeciesConversionsModuleConfig>().ReSampleInterspecies
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleIntraSpecies)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleIntraSpecies,
                        project.GetModuleConfiguration<IntraSpeciesFactorsModuleConfig>().ReSampleIntraSpecies
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleRPFs)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleRPFs,
                        project.GetModuleConfiguration<HazardCharacterisationsModuleConfig>().ReSampleRPFs
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleAssessmentGroupMemberships)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleAssessmentGroupMemberships,
                        project.GetModuleConfiguration<ActiveSubstancesModuleConfig>().ReSampleAssessmentGroupMemberships
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ReSampleImputationExposureDistributions)) {
                    section.SummarizeSetting(
                        SettingsItemType.ReSampleImputationExposureDistributions,
                        dietaryConfig.ReSampleImputationExposureDistributions
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleKineticModelParameters)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleKineticModelParameters,
                        project.GetModuleConfiguration<KineticModelsModuleConfig>().ResampleKineticModelParameters
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleHBMIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleHBMIndividuals,
                        project.GetModuleConfiguration<HumanMonitoringAnalysisModuleConfig>().ResampleHBMIndividuals
                    );
                }

                section.SummarizeSetting(SettingsItemType.UncertaintyLowerBound, actionConfig.UncertaintyLowerBound);
                section.SummarizeSetting(SettingsItemType.UncertaintyUpperBound, actionConfig.UncertaintyUpperBound);
            }

            return section;
        }
    }
}
