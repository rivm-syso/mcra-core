using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {

    public sealed class EnvironmentalBurdenOfDiseaseModuleSettings(ProjectDto project) {

        private readonly ProjectDto _project = project;

        public ExposureType ExposureType => _project.ActionSettings.ExposureType;
    }
}
