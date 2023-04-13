namespace MCRA.General.SettingsDefinitions {

    public enum SettingsItemType {
        Undefined = 0,

        // Agricultural use settings
        OccurrencePatternsTier,
        UseAgriculturalUseTable,
        UseOccurrenceFrequencies,
        UseOccurrencePatternsForResidueGeneration,
        SetMissingAgriculturalUseAsUnauthorized,
        UseAgriculturalUsePercentage,
        ScaleUpOccurencePatterns,
        RestrictOccurencePatternScalingToAuthorisedUses,

        // Amount/frequency model settings
        Covariate,
        Covariable,
        CovariateModelType,
        FunctionType,
        TestingLevel,
        TestingMethod,
        MinDegreesOfFreedom,
        MaxDegreesOfFreedom,

        // Assessment settings
        ExposureType,
        Aggregate,
        FocalCommodity,
        MultipleHealthEffects,
        TotalDietStudy,
        UseMonitoringDataForTDS,

        // Concentrations
        ConcentrationsTier,
        FilterConcentrationLimitExceedingSamples,
        ConcentrationLimitFilterFractionExceedanceThreshold,
        ExtrapolateConcentrations,
        ThresholdForExtrapolation,
        ConsiderMrlForExtrapolations,
        ConsiderAuthorisationsForExtrapolations,
        FilterSamplesByYear,
        AlignSampleDateSubsetWithPopulation,
        FilterSamplesByMonth,
        AlignSampleSeasonSubsetWithPopulation,
        FilterSamplesByLocation,
        AlignSampleLocationSubsetWithPopulation,
        FilterSamplesByRegion,
        AlignSampleRegionSubsetWithPopulation,
        FilterSamplesByProductionMethod,
        IncludeMissingDateValueRecords,
        IncludeMissingLocationRecords,
        IncludeMissingRegionRecords,
        IncludeMissingProductionMethodRecords,
        SampleMonths,
        SampleYears,
        SampleLocations,
        SampleRegions,
        SampleProductionMethods,
        

        // Concentration model settings
        UseComplexResidueDefinitions,
        UseDeterministicConversionFactors,
        IsDistribution,
        AllowHigherThanOne,
        ConcentrationModelChoice,
        DefaultConcentrationModel,
        IsFallbackMrl,
        FractionOfMrl,
        NonDetectsHandlingMethod,
        FractionOfLOR,
        RestrictLorImputationToAuthorisedUses,
        
        IsSampleBased,
        ImputeMissingValues,
        CorrelateImputedValueWithSamplePotency,
        IsSingleSamplePerDay,
        IsCorrelation,
        ConcentrationModelTypesPerFoodCompound,
        SubstanceTranslationAllocationMethod,
        ConsiderAuthorisationsForSubstanceConversion,
        RetainAllAllocatedSubstancesAfterAllocation,
        TryFixDuplicateAllocationInconsistencies,
        FocalCommodityReplacementMethod,
        FocalCommodityScenarioOccurrencePercentage,
        FocalCommodityConcentrationAdjustmentFactor,
        UseDeterministicSubstanceConversionsForFocalCommodity,
        FocalFoods,
        FocalSubstances,

        // Water imputation settings
        ImputeWaterConcentrations,
        CodeWater,
        WaterConcentrationValue,
        RestrictWaterImputationToMostPotentSubstances,
        RestrictWaterImputationToAuthorisedUses,
        RestrictWaterImputationToApprovedSubstances,

        // Scenario analysis settings
        UseScenario,
        IsMrlSettingScenario,
        ScenarioAnalysisSubstance,
        ScenarioAnalysisFoods,
        ProposedMrl,

        // Consumption data
        CodeFoodSurvey,
        LoopMultipleSurveys,

        // Dietary intake calculation settings
        DietaryIntakeCalculationTier,
        ImputeExposureDistributions,
        SingleValueDietaryExposureCalculationMethod,
        ModelledFoodsCalculationSource,
        VariabilityDiagnosticsAnalysis,

        // DoseResponseExperiments
        CodesExperiments,
        MergeDoseResponseExperimentsData,

        // Effect model
        RiskCalculationTier,
        HealthEffectType,
        LeftMargin,
        RightMargin,
        IsEAD,
        ThresholdMarginOfExposure,
        ConfidenceInterval,
        DefaultInterSpeciesFactorGeometricMean,
        DefaultInterSpeciesFactorGeometricStandardDeviation,
        DefaultIntraSpeciesFactor,
        NumberOfLabels,
        NumberOfSubstances,
        CumulativeRisk,
        RiskMetricType,
        IsInverseDistribution,
        UseAdjustmentFactors,
        ExposureAdjustmentFactorDistributionMethod,
        HazardAdjustmentFactorDistributionMethod,
        ExposureParameterA,
        ExposureParameterB,
        ExposureParameterC,
        ExposureParameterD,
        HazardParameterA,
        HazardParameterB,
        HazardParameterC,
        HazardParameterD,
        UseBackgroundAdjustmentFactor,
        CalculateRisksByFood,
        RiskMetricCalculationType,

        // Food conversion settings
        FoodIncludeNonDetects,
        CompoundIncludeNonDetects,
        CompoundIncludeNoMeasurements,
        UseProcessing,
        UseComposition,
        UseTdsComposition,
        UseReadAcrossFoodTranslations,
        UseMarketShares,
        UseSubTypes,
        UseSuperTypes,
        UseDefaultProcessingFactor,
        UseWorstCaseValues,
        DeriveModelledFoodsFromSampleBasedConcentrations,
        DeriveModelledFoodsFromSingleValueConcentrations,
        SubstanceIndependent,

        // Foods-as-measured subsets
        RestrictPopulationByModelledFoodSubset,
        FocalFoodAsMeasuredSubset,
        RestrictToModelledFoodSubset,
        ModelledFoodSubset,
        ModelledFoodsConsumerDaysOnly,

        // Foods-as-eaten subsets
        ConsumerDaysOnly,
        RestrictPopulationByFoodAsEatenSubset,
        FocalFoodAsEatenSubset,
        RestrictConsumptionsByFoodAsEatenSubset,
        FoodAsEatenSubset,
        IsDefaultSamplingWeight,
        UseHbmSamplingWeights,
        UseBodyWeightStandardisedConsumptionDistribution,
        ExcludeIndividualsWithLessThanNDays,
        MinimumNumberOfDays,

        // Population subset selection
        PopulationSubsetSelection,
        IsPopulationSubsetByGender,
        IsPopulationSubsetByAge,
        GenderSubsetSelection,
        MinimumAgeSubsetSelection,
        MaximumAgeSubsetSelection,
        LoopMultiplePopulations,
        MatchIndividualSubsetWithPopulation,
        SelectedFoodSurveySubsetProperties,
        MatchHbmIndividualSubsetWithPopulation,
        SelectedHbmSurveySubsetProperties,

        //Individual day subset selection
        FilterIndividualDaysByMonth,
        IncludeMissingIndividualDayRecords,
        IndividualDayMonths,

        // Sample subset
        SampleSubsetSelection,

        // Frequency model settings
        FrequencyModelCovariateModelType,
        FrequencyModelFunctionType,
        FrequencyModelTestingLevel,
        FrequencyModelTestingMethod,
        FrequencyModelMinDegreesOfFreedom,
        FrequencyModelMaxDegreesOfFreedom,

        // Intake model settings
        IntakeModelType,
        TransformType,
        GridPrecision,
        NumberOfIterations,
        SplineFit,
        CovariateModelling,
        FirstModelThenAdd,
        Dispersion,
        VarianceRatio,
        IntakeModelsPerCategory,

        // Kinetic model settings
        InternalModelType,
        OralAbsorptionFactorForDietaryExposure,
        OralAbsorptionFactor,
        DermalAbsorptionFactor,
        InhalationAbsorptionFactor,
        NonStationaryPeriod,
        NumberOfDosesPerDay,
        NumberOfDosesPerDayNonDietaryOral,
        NumberOfDosesPerDayNonDietaryDermal,
        NumberOfDosesPerDayNonDietaryInhalation,
        NumberOfDays,
        CodeCompartment,
        CodeKineticModel,
        UseParameterVariability,
        SpecifyEvents,
        SelectedEvents,

        // Mixture selection settings
        ExposureApproachType,
        NumberOfMixtures,
        MixtureSelectionSparsenessConstraint,
        MixtureSelectionIterations,
        MixtureSelectionRandomSeed,
        MixtureSelectionConvergenceCriterium,
        MixtureSelectionRatioCutOff,
        MixtureSelectionTotalExposureCutOff,
        InternalConcentrationType,
        ClusterMethodType,
        NumberOfClusters,
        AutomaticallyDeterminationOfClusters,
        NetworkAnalysisType,
        IsLogTransform,

        // Monte Carlo settings
        RandomSeed,
        NumberOfMonteCarloIterations,
        IsSurveySampling,

        // Output detail settings
        IsDetailedOutput,
        SummarizeSimulatedData,
        StoreIndividualDayIntakes,
        SelectedPercentiles,
        PercentageForDrilldown,
        PercentageForUpperTail,
        ExposureMethod,
        ExposureLevels,
        //Kan weg denk ik
        //ExposureInterpretation,
        Intervals,
        ExtraPredictionLevels,
        LowerPercentage,
        UpperPercentage,
        OutputSections,
        IsPerPerson,
        MaximumCumulativeRatioCutOff,
        MaximumCumulativeRatioPercentiles,
        MaximumCumulativeRatioMinimumPercentage,

        // NonDietary settings
        MatchSpecificIndividuals,
        IsCorrelationBetweenIndividuals,

        // Processing
        IsProcessing,

        // Effects
        CodeFocalEffect,
        MultipleEffects,
        IncludeAopNetwork,

        // AOP Networks
        CodeAopNetwork,
        RestrictAopByFocalUpstreamEffect,
        CodeFocalUpstreamEffect,

        // Active substances
        UseProbabilisticMemberships,
        AssessmentGroupMembershipCalculationMethod,
        PriorMembershipProbability,
        IncludeSubstancesWithUnknowMemberships,
        UseMolecularDockingModels,
        UseQsarModels,

        // Human monitoring
        CodesHumanMonitoringSurveys,
        CodesHumanMonitoringSamplingMethods,
        HumanMonitoringNonDetectsHandlingMethod,
        HumanMonitoringFractionOfLor,
        MissingValueImputationMethod,
        CorrelateTargetConcentrations,
        NonDetectImputationMethod,
        ImputeHbmConcentrationsFromOtherMatrices,
        HbmBetweenMatrixConversionFactor,
        MissingValueCutOff,
        StandardiseBlood,
        StandardiseBloodMethod,
        StandardiseUrine,
        StandardiseUrineMethod,
        McrExposureApproachType,
        IsMcrAnalysis,

        // Population
        CodePopulation,
        NominalPopulationBodyWeight,

        // Substances
        MultipleSubstances,
        MultipleSubstanceHandlingMethod,
        Cumulative,
        CodeReferenceCompound,
        CompoundGroupSelectionMethod,
        FilterByAvailableHazardDose,
        FilterByAvailableHazardCharacterisation,
        UseHazardCharacterisationsToDetermineActiveSubstances,
        FilterByCertainAssessmentGroupMembership,
        FilterByPossibleAssessmentGroupMembership,
        CombinationMethodMembershipInfoAndPodPresence,
        CodesSubstances,

        // Screening
        IsScreening,
        DietaryExposuresDetailsLevel,
        CriticalExposurePercentage,
        CumulativeSelectionPercentage,
        ImportanceLor,

        // Hazard characterisation settings
        RestrictToCriticalEffect,
        TargetDosesCalculationMethod,
        UseDoseResponseModels,
        ImputeMissingHazardDoses,
        HazardDoseImputationMethod,
        TargetDoseSelectionMethod,
        TargetDoseLevelType,
        PointOfDeparture,
        UseInterSpeciesConversionFactors,
        UseIntraSpeciesConversionFactors,
        UseAdditionalAssessmentFactor,
        AdditionalAssessmentFactor,

        // Uncertainty analysis settings
        DoUncertaintyAnalysis,
        NumberOfIterationsPerResampledSet,
        DoUncertaintyFactorial,
        UncertaintyLowerBound,
        UncertaintyUpperBound,
        NumberOfResampleCycles,
        ReSampleConcentrations,
        RecomputeOccurrencePatterns,
        IsParametric,
        UncertaintyType,
        ReSampleNonDietaryExposures,
        ResampleIndividuals,
        ReSamplePortions,
        ReSampleProcessingFactors,
        ReSampleAssessmentGroupMemberships,
        ReSampleImputationExposureDistributions,
        ReSampleRPFs,
        ReSampleInterspecies,
        ReSampleIntraSpecies,
        ReSampleParameterValues,
        ResampleKineticModelParameters,

        // Unit variability settings
        UseUnitVariability,
        UnitVariabilityModel,
        EstimatesNature,
        UnitVariabilityType,
        MeanValueCorrectionType,
        DefaultFactorLow,
        DefaultFactorMid,
        CorrelationType,

        // Responses
        CodeResponse,
        CodesResponses,

        //Single value risk settings
        SingleValueRiskCalculationMethod,
        Percentage
    }
}
