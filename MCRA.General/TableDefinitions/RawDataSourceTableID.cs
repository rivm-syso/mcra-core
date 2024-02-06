using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum RawDataSourceTableID {
        Unknown,
        /*** Foods ***/
        Foods,
        [Display(Name = "Food properties")]
        FoodProperties,
        [Display(Name = "Food origins")]
        FoodOrigins,
        [Display(Name = "Food translations")]
        FoodTranslations,
        [Display(Name = "Market shares")]
        MarketShares,
        [Display(Name = "Food hierarchies")]
        FoodHierarchies,
        [Display(Name = "Food facets")]
        Facets,
        [Display(Name = "Food facet descriptors")]
        FacetDescriptors,
        [Display(Name = "Food extrapolations")]
        FoodExtrapolations,
        /*** Consumptions ***/
        [Display(Name = "Consumption surveys")]
        FoodSurveys,
        [Display(Name = "Individuals")]
        Individuals,
        [Display(Name = "Individual days")]
        IndividualDays,
        [Display(Name = "Individual properties")]
        IndividualProperties,
        [Display(Name = "Individual property values")]
        IndividualPropertyValues,
        [Display(Name = "Consumptions")]
        Consumptions,
        [Display(Name = "Food consumption quantifications")]
        FoodConsumptionQuantifications,
        [Display(Name = "Food consumption quantification uncertainties")]
        FoodConsumptionUncertainties,
        /*** Compounds ***/
        [Display(Name = "Substances")]
        Compounds,
        /*** Concentrations ***/
        [Display(Name = "Food samples")]
        FoodSamples,
        [Display(Name = "Sample properties")]
        SampleProperties,
        [Display(Name = "Sample property values")]
        SamplePropertyValues,
        [Display(Name = "Analytical methods")]
        AnalyticalMethods,
        [Display(Name = "Analytical method substances")]
        AnalyticalMethodCompounds,
        [Display(Name = "Analysis samples")]
        AnalysisSamples,
        [Display(Name = "Sample concentrations")]
        ConcentrationsPerSample,
        [Display(Name = "Tabulated samples")]
        ConcentrationTabulated,
        [Display(Name = "SSD samples")]
        ConcentrationsSSD,
        [Display(Name = "Concentration distributions")]
        ConcentrationDistributions,
        /*** Processing factors ***/
        [Display(Name = "Processing types")]
        ProcessingTypes,
        [Display(Name = "Processing factors")]
        ProcessingFactors,
        [Display(Name = "Food unit weights")]
        FoodUnitWeights,
        /*** Unit variability factors***/
        [Display(Name = "Unit variability factors")]
        UnitVariabilityFactors,
        /*** Agricultural uses ***/
        [Display(Name = "Agricultural uses")]
        AgriculturalUses,
        [Display(Name = "Agricultural use substances")]
        AgriculturalUsesHasCompounds,
        /*** Maximum residue limits ***/
        [Display(Name = "Maximum residue limits")]
        MaximumResidueLimits,
        /*** Effects ***/
        [Display(Name = "Effects")]
        Effects,
        [Display(Name = "Test-systems")]
        TestSystems,
        [Display(Name = "Responses")]
        Responses,
        [Display(Name = "Relative potency factors")]
        RelativePotencyFactors,
        [Display(Name = "Relative potency factor uncertainties")]
        RelativePotencyFactorsUncertain,
        [Display(Name = "Hazard doses")]
        HazardDoses,
        [Display(Name = "Hazard dose uncertainties")]
        HazardDosesUncertain,
        [Display(Name = "Assessment group memberships")]
        AssessmentGroupMemberships,
        [Display(Name = "Assessment group membership models")]
        AssessmentGroupMembershipModels,
        [Display(Name = "Inter-species model parameters")]
        InterSpeciesModelParameters,
        [Display(Name = "Intra-species model parameters")]
        IntraSpeciesModelParameters,
        /*** Non-dietary exposures ***/
        [Display(Name = "Non-dietary exposure sources")]
        NonDietaryExposureSources,
        [Display(Name = "Non-dietary exposures")]
        NonDietaryExposures,
        [Display(Name = "Non-dietary exposure uncertainties")]
        NonDietaryExposuresUncertain,
        [Display(Name = "Non-dietary exposure surveys")]
        NonDietarySurveys,
        [Display(Name = "Non-dietary exposure survey properties")]
        NonDietarySurveyProperties,
        /*** Total diet studies ***/
        [Display(Name = "TDS food sample compositions")]
        TdsFoodSampleCompositions,
        /*** Dose response data ***/
        [Display(Name = "Dose response experiments")]
        DoseResponseExperiments,
        [Display(Name = "Dose response experiment doses")]
        DoseResponseExperimentDoses,
        [Display(Name = "Dose response experimental unit properties")]
        ExperimentalUnitProperties,
        [Display(Name = "Dose response experiment measurements")]
        DoseResponseExperimentMeasurements,
        [Display(Name = "Dose response data")]
        DoseResponseData,
        [Display(Name = "Kinetic model instance parameters")]
        KineticModelInstanceParameters,
        [Display(Name = "Kinetic model instances")]
        KineticModelInstances,
        [Display(Name = "Kinetic absorption factors")]
        KineticAbsorptionFactors,
        [Display(Name = "Kinetic model definitions")]
        KineticModelDefinitions,
        [Display(Name = "Kinetic model conversions")]
        KineticConversionFactors,
        [Display(Name = "Kinetic conversion factor subgroups")]
        KineticConversionFactorSGs,
        [Display(Name = "Dose response models")]
        DoseResponseModels,
        [Display(Name = "Dose response model benchmark doses")]
        DoseResponseModelBenchmarkDoses,
        [Display(Name = "Effect representations")]
        EffectRepresentations,
        [Display(Name = "AOP networks")]
        AdverseOutcomePathwayNetworks,
        [Display(Name = "Effect relations")]
        EffectRelations,
        [Display(Name = "Molecular docking models")]
        MolecularDockingModels,
        [Display(Name = "Molecular docking model binding energies")]
        MolecularBindingEnergies,
        [Display(Name = "QSAR membership models")]
        QsarMembershipModels,
        [Display(Name = "QSAR membership scores")]
        QsarMembershipScores,
        [Display(Name = "Human monitoring surveys")]
        HumanMonitoringSurveys,
        [Display(Name = "Human monitoring samples")]
        HumanMonitoringSamples,
        [Display(Name = "Human monitoring sample analyses")]
        HumanMonitoringSampleAnalyses,
        [Display(Name = "Human monitoring sample concentrations")]
        HumanMonitoringSampleConcentrations,
        [Display(Name = "Populations")]
        Populations,
        [Display(Name = "Substance translations")]
        ResidueDefinitions,
        [Display(Name = "Authorised uses")]
        AuthorisedUses,
        [Display(Name = "Dose response model benchmark doses uncertainty")]
        DoseResponseModelBenchmarkDosesUncertain,
        [Display(Name = "Dietary exposure models")]
        DietaryExposureModels,
        [Display(Name = "Dietary exposure percentiles")]
        DietaryExposurePercentiles,
        [Display(Name = "Dietary exposure percentile bootstraps")]
        DietaryExposurePercentilesUncertain,
        [Display(Name = "Hazard characterisations")]
        HazardCharacterisations,
        [Display(Name = "Hazard characterisations uncertainties")]
        HazardCharacterisationsUncertain,
        [Display(Name = "Hazard characterisations subgroups")]
        HCSubgroups,
        [Display(Name = "Hazard characterisations subgroup uncertainties")]
        HCSubgroupsUncertain,
        [Display(Name = "Target exposure models")]
        TargetExposureModels,
        [Display(Name = "Target exposure percentiles")]
        TargetExposurePercentiles,
        [Display(Name = "Target exposure percentile bootstraps")]
        TargetExposurePercentilesUncertain,
        [Display(Name = "Risk models")]
        RiskModels,
        [Display(Name = "Risk percentiles")]
        RiskPercentiles,
        [Display(Name = "Risk percentile bootstraps")]
        RiskPercentilesUncertain,
        [Display(Name = "Deterministic substance conversion factors")]
        DeterministicSubstanceConversionFactors,
        [Display(Name = "Population consumption single values")]
        PopulationConsumptionSingleValues,
        [Display(Name = "Concentration single values")]
        ConcentrationSingleValues,
        [Display(Name = "Occurrence frequencies")]
        OccurrenceFrequencies,
        [Display(Name = "IESTI special cases")]
        IestiSpecialCases,
        [Display(Name = "Populations individual property values")]
        PopulationIndividualPropertyValues,
        [Display(Name = "Substance approvals")]
        SubstanceApprovals,
        [Display(Name = "Exposure biological conversions")]
        ExposureBiomarkerConversions,
    }
}
