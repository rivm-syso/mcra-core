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
        Substances,
        Reference,
        Name,
        Description
    }

    public enum RawKineticAbsorptionFactors {
        IdCompound,
        Route,
        AbsorptionFactor,
    }

    public enum RawKineticConversionFactors {
        IdKineticConversionFactor,
        IdSubstanceFrom,
        ExposureRouteFrom,
        BiologicalMatrixFrom,
        DoseUnitFrom,
        ExpressionTypeFrom,
        IdSubstanceTo,
        ExposureRouteTo,
        BiologicalMatrixTo,
        DoseUnitTo,
        ExpressionTypeTo,
        ConversionFactor,
        UncertaintyDistributionType,
        UncertaintyUpper
    }

    public enum RawKineticConversionFactorSGs {
        IdKineticConversionFactor,
        ConversionFactor,
        AgeLower,
        Gender,
        UncertaintyUpper
    }
}
