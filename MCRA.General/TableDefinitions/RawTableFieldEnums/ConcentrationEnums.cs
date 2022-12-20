namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawAnalyticalMethodCompounds {
        IdAnalyticalMethod,
        IdCompound,
        LOD,
        LOQ,
        ConcentrationUnit
    }

    public enum RawAnalyticalMethods {
        IdAnalyticalMethod,
        Name,
        Description
    }

    public enum RawConcentrationsPerSample {
        IdAnalysisSample,
        IdCompound,
        Concentration,
        ResType
    }

    public enum RawFoodSamples {
        IdFoodSample,
        IdFood,
        Location,
        Region,
        DateSampling,
        ProductionMethod,
        Name,
        Description
    }

    public enum RawSampleProperties {
        Name,
        Description
    }

    public enum RawSamplePropertyValues {
        IdSample,
        PropertyName,
        TextValue,
        DoubleValue
    }

    public enum RawSampleYears {
        Year
    }

    public enum RawSampleLocations {
        Location
    }

    public enum RawSampleRegions {
        Region
    }

    public enum RawSampleProductionMethods {
        ProductionMethod
    }

    public enum RawAnalysisSamples {
        IdAnalysisSample,
        IdFoodSample,
        IdAnalyticalMethod,
        DateAnalysis,
        Name, 
        Description
    }

    public enum RawTwoWayTableData {
        TwoWayTableRecordsData
    }

    public enum RawConcentrationSingleValues {
        IdFood,
        IdSubstance,
        Value,
        ValueType,
        Percentile,
        ConcentrationUnit,
        Reference
    }
}
