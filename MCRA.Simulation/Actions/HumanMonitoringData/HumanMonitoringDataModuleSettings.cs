using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Action.Settings.Dto;
using System.Collections.Generic;

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

        public List<string> SurveyCodes {
            get {
                return _project.HumanMonitoringSettings.SurveyCodes;
            }
        }

        public List<string> SamplingMethodCodes {
            get {
                return _project.HumanMonitoringSettings.SamplingMethodCodes;
            }
        }

        public List<IndividualsSubsetDefinitionDto> IndividualsSubsetDefinitions {
            get {
                return _project.IndividualsSubsetDefinitions;
            }
        }

        public IndividualDaySubsetDefinitionDto IndividualDaySubsetDefinition {
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
    }
}
