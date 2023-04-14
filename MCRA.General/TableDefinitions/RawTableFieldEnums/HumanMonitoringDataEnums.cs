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
        IdPopulation,
        LipidConcentrationUnit,
        TriglycConcentrationUnit,
        CholestConcentrationUnit,
        CreatConcentrationUnit
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
        Description,
        LipidGrav,
        LipidEnz,
        Triglycerides,
        Cholesterol,
        Creatinine,
        OsmoticConcentration
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
