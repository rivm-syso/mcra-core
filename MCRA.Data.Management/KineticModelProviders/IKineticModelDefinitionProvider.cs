using MCRA.General;

namespace MCRA.Data.Management.CompiledDataManagers.PbkModelProviders {
    public interface IKineticModelDefinitionProvider {

        HashSet<string> CodesAvailableKineticModelDefinition();

        bool TryGetKineticModelDefinition(string code, out KineticModelDefinition kineticModelDefinition);

    }
}
