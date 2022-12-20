namespace MCRA.General.TableDefinitions.RawTableFieldEnums {

    public enum RawDietaryExposureModels {
        IdDietaryExposureModel,
        Name,
        Description,
        IdSubstance,
        ExposureUnit
    }

    public enum RawDietaryExposurePercentiles {
        IdDietaryExposureModel,
        Percentage,
        Exposure
    }

    public enum RawDietaryExposurePercentileUncertains {
        IdDietaryExposureModel,
        IdUncertaintySet,
        Percentage,
        Exposure
    }
}
