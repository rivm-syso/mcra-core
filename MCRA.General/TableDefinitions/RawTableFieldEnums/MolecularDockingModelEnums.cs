namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawMolecularDockingModels {
        Id,
        Name,
        Description,
        IdEffect,
        Threshold,
        NumberOfReceptors,
        Reference,
    }

    public enum RawMolecularBindingEnergies {
        IdMolecularDockingModel,
        IdCompound,
        BindingEnergy
    }
}
