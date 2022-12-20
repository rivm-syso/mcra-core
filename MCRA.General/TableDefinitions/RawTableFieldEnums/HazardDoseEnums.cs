namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawHazardDoses {
        IdDoseResponseModel,
        IdEffect,
        IdCompound,
        Species,
        ModelCode,
        DoseResponseModelEquation,
        DoseResponseModelParameterValues,
        LimitDose,
        HazardDoseType,
        DoseUnit,
        CriticalEffectSize,
        ExposureRoute,
        IsCriticalEffect
    }

    public enum RawHazardDosesUncertain {
        IdDoseResponseModel,
        IdUncertaintySet,
        IdEffect,
        IdCompound,
        LimitDose,
        DoseResponseModelParameterValues
    }
}
