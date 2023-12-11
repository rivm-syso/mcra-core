namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawHazardCharacterisations {
        IdHazardCharacterisation,
        IdEffect,
        IdSubstance,
        IdPopulationType,
        TargetLevel,
        ExposureRoute,
        TargetOrgan,
        ExpressionType,
        IsCriticalEffect,
        ExposureType,
        HazardCharacterisationType,
        Qualifier,
        Value,
        DoseUnit,
        IdPointOfDeparture,
        CombinedAssessmentFactor,
        PublicationTitle,
        PublicationAuthors,
        PublicationYear,
        PublicationUri,
        Name,
        Description
    }

    public enum RawHazardCharacterisationsUncertain {
        IdHazardCharacterisation,
        IdSubstance,
        Value,
    }

    public enum RawHCSubgroups {
        IdSubgroup,
        IdHazardCharacterisation,
        IdSubstance,
        AgeLower,
        Gender,
        Value,
    }
    public enum RawHCSubgroupsUncertain {
        IdSubgroup,
        IdHazardCharacterisation,
        idUncertaintySet,
        Value,
    }
}
