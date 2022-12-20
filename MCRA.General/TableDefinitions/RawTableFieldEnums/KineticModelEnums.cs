namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawKineticModelInstanceParameters {
        IdModelInstance,
        Parameter,
        Description,
        Value,
        DistributionType,
        CvVariability,
        CvUncertainty,
    }

    public enum RawKineticModelInstances {
        IdModelInstance,
        IdModelDefinition,
        IdTestSystem,
        IdSubstance,
        Reference,
        Name,
        Description
    }

    public enum RawKineticAbsorptionFactors {
        IdCompound,
        Route,
        AbsorptionFactor,
    }
}
