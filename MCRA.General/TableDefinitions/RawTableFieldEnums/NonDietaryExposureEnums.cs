namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawNonDietarySurveys {
        IdNonDietarySurvey,
        Name,
        Description,
        Location,
        Date,
        NonDietaryIntakeUnit,
        ProportionZeros,
        IdPopulation,
    }

    public enum RawNonDietarySurveyProperties {
        IndividualPropertyName,
        IdNonDietarySurvey,
        IndividualPropertyTextValue,
        IndividualPropertyDoubleValueMin,
        IndividualPropertyDoubleValueMax
    }

    public enum RawNonDietaryExposures {
        IdIndividual,
        IdNonDietarySurvey,
        IdCompound,
        Dermal,
        Oral,
        Inhalation
    }

    public enum RawNonDietaryExposuresUncertain {
        IdIndividual,
        IdNonDietarySurvey,
        IdCompound,
        Id,
        Dermal,
        Oral,
        Inhalation
    }
}
