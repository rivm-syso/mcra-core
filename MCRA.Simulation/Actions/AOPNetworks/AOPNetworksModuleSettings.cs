using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.AOPNetworks {

    public sealed class AOPNetworksModuleSettings {

        private readonly AOPNetworksModuleConfig _configuration;

        public AOPNetworksModuleSettings(AOPNetworksModuleConfig configuration) {
            _configuration = configuration;
        }

        public string CodeAopNetwork => _configuration.CodeAopNetwork;

        public bool RestrictAopByFocalUpstreamEffect => _configuration.RestrictAopByFocalUpstreamEffect;

        public string CodeFocalUpstreamEffect => _configuration.CodeFocalUpstreamEffect;
    }
}
