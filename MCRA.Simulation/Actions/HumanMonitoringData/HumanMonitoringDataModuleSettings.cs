using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringData {

    public sealed class HumanMonitoringDataModuleSettings {

        private readonly HumanMonitoringDataModuleConfig _configuration;

        public HumanMonitoringDataModuleSettings(HumanMonitoringDataModuleConfig config) {
            _configuration = config;
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public List<string> SamplingMethodCodes {
            get {
                return _configuration.CodesHumanMonitoringSamplingMethods;
            }
        }

        public List<IndividualsSubsetDefinition> IndividualsSubsetDefinitions {
            get {
                return _configuration.IndividualsSubsetDefinitions;
            }
        }

        public IndividualDaySubsetDefinition IndividualDaySubsetDefinition {
            get {
                return _configuration.IndividualDaySubsetDefinition;
            }
        }

        public IndividualSubsetType MatchHbmIndividualSubsetWithPopulation {
            get {
                return _configuration.MatchHbmIndividualSubsetWithPopulation;
            }
        }

        public bool UseHbmSamplingWeights {
            get {
                return _configuration.UseHbmSamplingWeights;
            }
        }

        public List<string> SelectedHbmSurveySubsetProperties {
            get {
                if (MatchHbmIndividualSubsetWithPopulation == IndividualSubsetType.MatchToPopulationDefinitionUsingSelectedProperties) {
                    return _configuration.SelectedHbmSurveySubsetProperties;
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
                return _configuration.UseCompleteAnalysedSamples;
            }
        }

        public bool ExcludeSubstancesFromSamplingMethod {
            get {
                return _configuration.ExcludeSubstancesFromSamplingMethod;
            }
        }

        public List<HbmSamplingMethodSubstance> ExcludedSubstancesFromSamplingMethodSubset {
            get {
                return _configuration.ExcludedSubstancesFromSamplingMethodSubset;
            }
        }

        public bool FilterRepeatedMeasurements {
            get {
                return _configuration.FilterRepeatedMeasurements;
            }
        }

        public List<string> RepeatedMeasurementTimepointCodes {
            get {
                return _configuration.RepeatedMeasurementTimepointCodes;
            }
        }
    }
}
