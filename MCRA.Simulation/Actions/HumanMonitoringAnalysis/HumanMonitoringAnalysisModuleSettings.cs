using MCRA.General;
using MCRA.General.Action.Settings.Dto;

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

        public bool ImputeHbmConcentrationsFromOtherMatrices => _project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices;

        public double HbmBetweenMatrixConversionFactor => _project.HumanMonitoringSettings.HbmBetweenMatrixConversionFactor;

        public double MissingValueCutOff => _project.HumanMonitoringSettings.MissingValueCutOff;

        public bool StandardiseBlood => _project.HumanMonitoringSettings.StandardiseBlood;

        public StandardiseBloodMethod StandardiseBloodMethod => _project.HumanMonitoringSettings.StandardiseBloodMethod;

        public bool StandardiseUrine => _project.HumanMonitoringSettings.StandardiseUrine;

        public StandardiseUrineMethod StandardiseUrineMethod => _project.HumanMonitoringSettings.StandardiseUrineMethod;
    }
}
