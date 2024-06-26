﻿using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    public sealed class HumanMonitoringDataModuleSettings {

        private readonly ProjectDto _project;

        public HumanMonitoringDataModuleSettings(ProjectDto project) {
            _project = project;
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public List<string> SamplingMethodCodes {
            get {
                return _project.HumanMonitoringSettings.SamplingMethodCodes;
            }
        }

        public List<IndividualsSubsetDefinition> IndividualsSubsetDefinitions {
            get {
                return _project.IndividualsSubsetDefinitions;
            }
        }

        public IndividualDaySubsetDefinition IndividualDaySubsetDefinition {
            get {
                return _project.IndividualDaySubsetDefinition;
            }
        }

        public IndividualSubsetType MatchHbmIndividualSubsetWithPopulation {
            get {
                return _project.SubsetSettings.MatchHbmIndividualSubsetWithPopulation;
            }
        }

        public bool UseHbmSamplingWeights {
            get {
                return _project.SubsetSettings.UseHbmSamplingWeights;
            }
        }

        public List<string> SelectedHbmSurveySubsetProperties {
            get {
                if (MatchHbmIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                    return _project.SelectedHbmSurveySubsetProperties;
                } else {
                    return null;
                }
            }
        }

        public ConcentrationUnit DefaultConcentrationUnit {
            get {
                return ConcentrationUnit.ugPerL;
            }
        }

        public bool UseCompleteAnalysedSamples {
            get {
                return _project.HumanMonitoringSettings.UseCompleteAnalysedSamples;
            }
        }

        public bool ExcludeSubstancesFromSamplingMethod {
            get {
                return _project.HumanMonitoringSettings.ExcludeSubstancesFromSamplingMethod;
            }
        }

        public List<HbmSamplingMethodSubstance> ExcludedSubstancesFromSamplingMethodSubset {
            get {
                return _project.HumanMonitoringSettings.ExcludedSubstancesFromSamplingMethodSubset;
            }
        }

        public bool FilterRepeatedMeasurements {
            get {
                return _project.HumanMonitoringSettings.FilterRepeatedMeasurements;
            }
        }

        public List<string> RepeatedMeasurementTimepointCodes {
            get {
                return _project.HumanMonitoringSettings.RepeatedMeasurementTimepointCodes;
            }
        }
    }
}
