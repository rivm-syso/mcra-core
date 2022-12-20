namespace MCRA.General.TableDefinitions.RawTableFieldEnums {

    public enum RawTargetExposureModels {
        IdTargetExposureModel,
        Name,
        Description,
        IdSubstance,
        ExposureUnit
    }

    public enum RawTargetExposurePercentiles {
        IdTargetExposureModel,
        Percentage,
        Exposure
    }

    public enum RawTargetExposurePercentileUncertains {
        IdTargetExposureModel,
        IdUncertaintySet,
        Percentage,
        Exposure
    }
}
