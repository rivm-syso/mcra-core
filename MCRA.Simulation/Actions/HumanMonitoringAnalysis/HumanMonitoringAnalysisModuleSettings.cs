using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public sealed class HumanMonitoringAnalysisModuleSettings {

        private readonly ProjectDto _project;
        private readonly bool _isUncertaintyCycle;
        public HumanMonitoringAnalysisModuleSettings(ProjectDto project, bool isUncertaintyCycle) {
            _project = project;
            _isUncertaintyCycle = isUncertaintyCycle;
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public NonDetectsHandlingMethod NonDetectsHandlingMethod => _project.HumanMonitoringSettings.NonDetectsHandlingMethod;

        public double LorReplacementFactor => _project.HumanMonitoringSettings.FractionOfLor;

        public MissingValueImputationMethod MissingValueImputationMethod => _project.HumanMonitoringSettings.MissingValueImputationMethod;

        public NonDetectImputationMethod NonDetectImputationMethod => _project.HumanMonitoringSettings.NonDetectImputationMethod;

        public BiologicalMatrix TargetMatrix => _project.HumanMonitoringSettings.TargetMatrix;

        public bool HbmConvertToSingleTargetMatrix => _project.HumanMonitoringSettings.HbmConvertToSingleTargetMatrix;

        public KineticConversionType KineticConversionMethod => _project.HumanMonitoringSettings.KineticConversionMethod;
        
        public double HbmBetweenMatrixConversionFactor => _project.HumanMonitoringSettings.HbmBetweenMatrixConversionFactor;

        public double MissingValueCutOff => _project.HumanMonitoringSettings.MissingValueCutOff;

        public bool StandardiseBlood => _project.HumanMonitoringSettings.StandardiseBlood;

        public StandardiseBloodMethod StandardiseBloodMethod => _project.HumanMonitoringSettings.StandardiseBloodMethod;

        public bool StandardiseBloodExcludeSubstances => _project.HumanMonitoringSettings.StandardiseBloodExcludeSubstances;

        public List<string> StandardiseBloodExcludedSubstancesSubset => _project.HumanMonitoringSettings.StandardiseBloodExcludedSubstancesSubset;

        public bool StandardiseUrine => _project.HumanMonitoringSettings.StandardiseUrine;

        public StandardiseUrineMethod StandardiseUrineMethod => _project.HumanMonitoringSettings.StandardiseUrineMethod;

        public bool StandardiseUrineExcludeSubstances => _project.HumanMonitoringSettings.StandardiseUrineExcludeSubstances;

        public List<string> StandardiseUrineExcludedSubstancesSubset => _project.HumanMonitoringSettings.StandardiseUrineExcludedSubstancesSubset;

        public int NumberOfMonteCarloIterations {
            get {
                if (!_isUncertaintyCycle) {
                    return _project.MonteCarloSettings.NumberOfMonteCarloIterations;
                } else {
                    return _project.UncertaintyAnalysisSettings.NumberOfIterationsPerResampledSet;
                }
            }
        }
    }
}
