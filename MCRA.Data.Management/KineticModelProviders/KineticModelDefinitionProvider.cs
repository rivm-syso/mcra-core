using MCRA.Data.Management.CompiledDataManagers.PbkModelProviders;
using MCRA.General;

namespace MCRA.Data.Management.CompiledDataManagers {
    public class KineticModelDefinitionProvider : IKineticModelDefinitionProvider {

        public bool TryGetKineticModelDefinition(string code, out KineticModelDefinition kineticModelDefinition) {
            return MCRAKineticModelDefinitions.TryGetDefinitionByAlias(code, out kineticModelDefinition);
        }

        public HashSet<string> CodesAvailableKineticModelDefinition() {
            var codes = MCRAKineticModelDefinitions.Definitions
                .Select(r => r.Value.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            codes.UnionWith(MCRAKineticModelDefinitions.Definitions
                .Where(r => r.Value.Aliases != null)
                .SelectMany(r => r.Value.Aliases));
            return codes;
        }
    }
}
