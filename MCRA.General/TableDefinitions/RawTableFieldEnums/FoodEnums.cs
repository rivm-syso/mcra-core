namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawFoods {
        IdFood,
        Name,
        AlternativeName,
        Description
    }

    public enum RawFoodProperties {
        IdFood,
        UnitWeight,
        LargePortion,
        LargePortionBabies,
        LargePortionChildren
    }

    public enum RawFoodUnitWeights {
        IdFood,
        Location,
        ValueType,
        Qualifier,
        Value,
        Reference
    }

    public enum RawFoodHierarchies {
        IdFood,
        IdParent
    }

    public enum RawFoodOrigins {
        IdFood,
        MarketLocation,
        OriginLocation,
        StartDate,
        EndDate,
        Percentage
    }

    public enum RawFacetDescriptors {
        IdFacetDescriptor,
        Name,
        Description
    }

    public enum RawFacets {
        IdFacet,
        Name,
        Description
    }

    public enum RawProcessingTypes {
        IdProcessingType,
        Name,
        Description,
        DistributionType,
        BulkingBlending
    }
}
