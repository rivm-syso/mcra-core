namespace MCRA.General.TableDefinitions.RawTableFieldEnums {
    public enum RawHumanMonitoringSurveys {
        IdSurvey,
        Name,
        Description,
        Location,
        BodyWeightUnit,
        AgeUnit,
        StartDate,
        EndDate,
        Year,
        NumberOfSurveyDays,
        IdPopulation
    }

    public enum RawHumanMonitoringSamples {
        IdSample,
        IdIndividual,
        DateSampling,
        DayOfSurvey,
        TimeOfSampling,
        SampleType,
        Compartment,
        ExposureRoute,
        SpecificGravity,
        SpecificGravityCorrectionFactor,
        Name,
        Description
    }

    public enum RawHumanMonitoringSampleAnalyses {
        IdSampleAnalysis,
        IdSample,
        IdAnalyticalMethod,
        DateAnalysis,
        Name,
        Description
    }

    public enum RawHumanMonitoringSampleConcentrations {
        IdAnalysisSample,
        IdCompound,
        Concentration,
        ResType
    }
}
