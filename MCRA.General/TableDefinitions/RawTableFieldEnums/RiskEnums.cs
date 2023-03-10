namespace MCRA.General.TableDefinitions.RawTableFieldEnums {

    public enum RawRiskModels {
        IdRiskModel,
        Name,
        Description,
        IdSubstance,
        RiskMetric,
    }

    public enum RawRiskPercentiles {
        IdRiskModel,
        Percentage,
        Risk
    }

    public enum RawRiskPercentileUncertains {
        IdRiskModel,
        IdUncertaintySet,
        Percentage,
        Risk
    }
}
