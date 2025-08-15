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
                if (mapping.RawDataSources?.Count > 0) {
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
            var actionConfig = project.ActionSettings;
            var dietaryConfig = project.DietaryExposuresSettings;

            if ((project.DietaryExposuresSettings.IsCompute
                || project.TargetExposuresSettings.IsCompute
                || project.RisksSettings.IsCompute
                )
                || (actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.Risks, out var moduleMapping) && moduleMapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.DietaryExposures, out moduleMapping) && moduleMapping.IsCompute
                || actionMapping.ModuleMappingsDictionary.TryGetValue(ActionType.TargetExposures, out moduleMapping) && moduleMapping.IsCompute)
            ) {
                section.SummarizeSetting(SettingsItemType.SelectedPercentiles, dietaryConfig.SelectedPercentiles);
                section.SummarizeSetting(SettingsItemType.ExposureLevels, dietaryConfig.ExposureLevels);
                section.SummarizeSetting(SettingsItemType.ExposureMethod, dietaryConfig.ExposureMethod);

                if (dietaryConfig.IntakeCovariateModelling) {
                    section.SummarizeSetting(SettingsItemType.IntakeModelPredictionIntervals, dietaryConfig.IntakeModelPredictionIntervals);
                    section.SummarizeSetting(SettingsItemType.IntakeExtraPredictionLevels, dietaryConfig.IntakeExtraPredictionLevels);
                }
            }

            if (actionMapping.OutputSettings.Contains(SettingsItemType.SkipPrivacySensitiveOutputs)) {
                section.SummarizeSetting(SettingsItemType.SkipPrivacySensitiveOutputs, actionConfig.SkipPrivacySensitiveOutputs);
                if (actionMapping.OutputSettings.Contains(SettingsItemType.StoreIndividualDayIntakes)) {
                    section.SummarizeSetting(
                        SettingsItemType.StoreIndividualDayIntakes,
                        project.TargetExposuresSettings.StoreIndividualDayIntakes
                    );
                }
                if (actionMapping.OutputSettings.Contains(SettingsItemType.IsDetailedOutput)) {
                    section.SummarizeSetting(SettingsItemType.IsDetailedOutput, dietaryConfig.IsDetailedOutput);
                    section.SummarizeSetting(SettingsItemType.VariabilityDrilldownPercentage, dietaryConfig.VariabilityDrilldownPercentage);
                }
            }

            section.SummarizeSetting(SettingsItemType.VariabilityUpperTailPercentage, actionConfig.VariabilityUpperTailPercentage);
            section.SummarizeSetting(SettingsItemType.VariabilityLowerPercentage, actionConfig.VariabilityLowerPercentage);
            section.SummarizeSetting(SettingsItemType.VariabilityUpperPercentage, actionConfig.VariabilityUpperPercentage);
            section.SummarizeSetting(SettingsItemType.IsPerPerson, dietaryConfig.IsPerPerson);
            return section;
        }

        public ActionSettingsSummary SummarizeRunSettings(ProjectDto project) {
            var section = new ActionSettingsSummary("Initialisation seed");
            section.SummarizeSetting(SettingsItemType.RandomSeed, project.ActionSettings.RandomSeed);
            return section;
        }

        public ActionSettingsSummary SummarizeUncertainty(ProjectDto project, ActionMapping actionMapping) {
            var section = new ActionSettingsSummary("Uncertainty settings");
            var actionConfig = project.ActionSettings;
            var dietaryConfig = project.DietaryExposuresSettings;

            section.SummarizeSetting(SettingsItemType.DoUncertaintyAnalysis, actionConfig.DoUncertaintyAnalysis);
            if (actionConfig.DoUncertaintyAnalysis) {
                section.SummarizeSetting(SettingsItemType.UncertaintyAnalysisCycles, actionConfig.UncertaintyAnalysisCycles);

                var activeUncertaintySettings = actionMapping.AvailableUncertaintySources;

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.UncertaintyIterationsPerResampledSet,
                        dietaryConfig.UncertaintyIterationsPerResampledSet
                    );
                }

                if (actionMapping.ModuleDefinition.HasUncertaintyFactorial) {
                    section.SummarizeSetting(SettingsItemType.DoUncertaintyFactorial, actionConfig.DoUncertaintyFactorial);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleConcentrations)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleConcentrations,
                        project.ConcentrationsSettings.ResampleConcentrations
                    );
                    section.SummarizeSetting(
                        SettingsItemType.IsParametric,
                        project.ConcentrationModelsSettings.IsParametric
                    );
                    if (activeUncertaintySettings.Contains(SettingsItemType.RecomputeOccurrencePatterns)) {
                        section.SummarizeSetting(
                            SettingsItemType.RecomputeOccurrencePatterns,
                            project.OccurrencePatternsSettings.RecomputeOccurrencePatterns
                        );
                    }
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleCPConcentrations)) {
                    section.SummarizeSetting(SettingsItemType.ResampleCPConcentrations, project.ConsumerProductConcentrationDistributionsSettings.ResampleCPConcentrations);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleAirConcentrations)) {
                    section.SummarizeSetting(SettingsItemType.ResampleAirConcentrations, project.IndoorAirConcentrationsSettings.ResampleAirConcentrations);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleDustConcentrations)) {
                    section.SummarizeSetting(SettingsItemType.ResampleDustConcentrations, project.DustConcentrationDistributionsSettings.ResampleDustConcentrations);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleSoilConcentrations)) {
                    section.SummarizeSetting(SettingsItemType.ResampleSoilConcentrations, project.SoilConcentrationDistributionsSettings.ResampleSoilConcentrations);
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleIndividuals,
                        project.ConsumptionsSettings.ResampleIndividuals
                    );
                }
                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleSimulatedIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleSimulatedIndividuals,
                        project.IndividualsSettings.ResampleSimulatedIndividuals
                    );
                }
                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleProcessingFactors)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleProcessingFactors,
                        project.ProcessingFactorsSettings.ResampleProcessingFactors
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleNonDietaryExposures)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleNonDietaryExposures,
                        project.NonDietaryExposuresSettings.ResampleNonDietaryExposures
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleInterspecies)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleInterspecies,
                        project.InterSpeciesConversionsSettings.ResampleInterspecies
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleIntraSpecies)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleIntraSpecies,
                        project.IntraSpeciesFactorsSettings.ResampleIntraSpecies
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleRPFs)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleRPFs,
                        project.HazardCharacterisationsSettings.ResampleRPFs
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleAssessmentGroupMemberships)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleAssessmentGroupMemberships,
                        project.ActiveSubstancesSettings.ResampleAssessmentGroupMemberships
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleImputationExposureDistributions)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleImputationExposureDistributions,
                        dietaryConfig.ResampleImputationExposureDistributions
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleKineticConversionFactors)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleKineticConversionFactors,
                        project.KineticConversionFactorsSettings.ResampleKineticConversionFactors
                    );
                }
                if (activeUncertaintySettings.Contains(SettingsItemType.ResamplePbkModelParameters)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResamplePbkModelParameters,
                        project.PbkModelsSettings.ResamplePbkModelParameters
                    );
                }

                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleHbmIndividuals)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleHbmIndividuals,
                        project.HumanMonitoringAnalysisSettings.ResampleHbmIndividuals
                    );
                }
                if (activeUncertaintySettings.Contains(SettingsItemType.ResampleExposureResponseFunctions)) {
                    section.SummarizeSetting(
                        SettingsItemType.ResampleExposureResponseFunctions,
                        project.ExposureResponseFunctionsSettings.ResampleExposureResponseFunctions
                    );
                }

                section.SummarizeSetting(SettingsItemType.UncertaintyLowerBound, actionConfig.UncertaintyLowerBound);
                section.SummarizeSetting(SettingsItemType.UncertaintyUpperBound, actionConfig.UncertaintyUpperBound);
            }

            return section;
        }
    }
}
