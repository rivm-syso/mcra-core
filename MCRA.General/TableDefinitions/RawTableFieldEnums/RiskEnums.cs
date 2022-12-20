namespace MCRA.General.TableDefinitions.RawTableFieldEnums {

    public enum RawRiskModels {
        IdRiskModel,
        Name,
        Description,
        IdSubstance,
    }

    public enum RawRiskPercentiles {
        IdRiskModel,
        Percentage,
        MarginOfExposure
    }

    public enum RawRiskPercentileUncertains {
        IdRiskModel,
        IdUncertaintySet,
        Percentage,
        MarginOfExposure
    }
}
