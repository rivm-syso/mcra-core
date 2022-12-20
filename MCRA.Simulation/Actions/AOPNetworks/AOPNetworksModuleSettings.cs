using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Actions.AOPNetworks {

    public sealed class AOPNetworksModuleSettings {

        private readonly ProjectDto _project;

        public AOPNetworksModuleSettings(ProjectDto project) {
            _project = project;
        }

        public string CodeAopNetwork {
            get {
                return _project.EffectSettings.CodeAopNetwork;
            }
        }

        public bool RestrictAopByFocalUpstreamEffect {
            get {
                return _project.EffectSettings.RestrictAopByFocalUpstreamEffect;
            }
        }

        public string CodeFocalUpstreamEffect {
            get {
                return _project.EffectSettings.CodeFocalUpstreamEffect;
            }
        }
    }
}
