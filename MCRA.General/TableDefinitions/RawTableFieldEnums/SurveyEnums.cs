namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawFoodSurveys {
        IdFoodSurvey,
        Name,
        Description,
        Year,
        Location,
        BodyWeightUnit,
        AgeUnit,
        ConsumptionUnit,
        StartDate,
        EndDate,
        NumberOfSurveyDays,
        IdPopulation
    }

    public enum RawIndividualDays {
        IdIndividual,
        IdDay,
        SamplingDate
    }

    public enum RawIndividuals {
        IdIndividual,
        IdFoodSurvey,
        BodyWeight,
        SamplingWeight,
        NumberOfSurveyDays,
        Name,
        Description
    }

    public enum RawIndividualProperties {
        IdIndividualProperty,
        Name, 
        PropertyLevel,
        Description,
        Type
    }

    public enum RawIndividualPropertyValues {
        IdIndividual,
        PropertyName,
        TextValue,
        DoubleValue
    }

    public enum RawFoodConsumptionQuantifications {
        IdFood,
        IdUnit,
        UnitWeight,
        UnitWeightUncertainty,
        AmountUncertainty
    }

    public enum RawFoodConsumptions {
        IdIndividual,
        IdFood,
        Facets,
        IdUnit,
        IdDay,
        IdMeal,
        Amount,
        DateConsumed,
    }

    public enum RawFoodConsumptionUncertainties {
        IdFood,
        IdUnit,
        UnitWeightUncertainty,
        AmountUncertainty
    }
}
