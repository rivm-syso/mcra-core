namespace MCRA.General.TableDefinitions.RawTableFieldEnums {

    public enum RawAdverseOutcomePathwayNetworks {
        IdAdverseOutcomePathwayNetwork,
        Name,
        Description,
        Reference,
        IdAdverseOutcome,
        RiskType
    }

    public enum RawEffectRelations {
        IdAdverseOutcomePathwayNetwork,
        IdDownstreamKeyEvent,
        IdUpstreamKeyEvent,
        Reference,
    }
}
