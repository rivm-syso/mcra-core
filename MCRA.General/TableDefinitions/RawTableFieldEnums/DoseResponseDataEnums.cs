namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawDoseResponseExperiments {
        IdExperiment,
        Name,
        Description,
        Date,
        Reference,
        ExperimentalUnit,
        DoseRoute,
        Substances,
        DoseUnit,
        Responses,
        Time,
        TimeUnit,
        Covariates
    }

    public enum RawDoseResponseExperimentDoses {
        IdExperiment,
        IdExperimentalUnit,
        Time,
        IdSubstance,
        Dose
    }

    public enum RawDoseResponseExperimentMeasurements {
        IdExperiment,
        IdExperimentalUnit,
        IdResponse,
        Time,
        ResponseValue,
        ResponseSD,
        ResponseCV,
        ResponseN,
        ResponseUncertaintyUpper
    }
   
    public enum RawExperimentalUnitProperties {
        IdExperiment,
        IdExperimentalUnit,
        PropertyName,
        Value
    }
}
