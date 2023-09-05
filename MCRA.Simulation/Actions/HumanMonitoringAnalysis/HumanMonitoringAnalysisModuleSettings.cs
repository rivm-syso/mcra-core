using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public sealed class HumanMonitoringAnalysisModuleSettings {

        private readonly ProjectDto _project;

        public HumanMonitoringAnalysisModuleSettings(ProjectDto project) {
            _project = project;
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

        public bool ConvertToSingleMatrix => true;

        public BiologicalMatrix TargetMatrix => _project.HumanMonitoringSettings.TargetMatrix;

        public bool ImputeHbmConcentrationsFromOtherMatrices => _project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices;

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
    }
}
