namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawQSARMembershipModels {
        Id,
        Name,
        Description,
        IdEffect,
        Accuracy,
        Sensitivity,
        Specificity,
        Reference,
    }

    public enum RawQSARMembershipScores {
        IdQSARMembershipModel,
        IdSubstance,
        MembershipScore
    }
}
