namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawDoseResponseModels {
        IdDoseResponseModel,
        IdExperiment,
        Name,
        Description,
        Substances,
        IdResponse,
        Covariates,
        CriticalEffectSize,
        BenchmarkResponseType,
        LogLikelihood,
        DoseUnit,
        ModelEquation,
        ModelParameterValues
    }

    public enum RawDoseResponseModelBenchmarkDoses {
        IdDoseResponseModel,
        IdSubstance,
        Covariates,
        BenchmarkResponse,
        BenchmarkDose,
        BenchmarkDoseLower,
        BenchmarkDoseUpper,
        ModelParameterValues
    }

    public enum RawDoseResponseModelBenchmarkDosesUncertain {
        IdDoseResponseModel,
        IdUncertaintySet,
        IdSubstance,
        Covariates,
        BenchmarkDose
    }
}
