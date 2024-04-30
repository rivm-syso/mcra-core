using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ModuleType {
        PrimaryEntityModule,
        DataModule,
        CalculatorModule,
        SupportModule
    }

    public enum ActionClass {
        [Display(Name = "Primary entity")]
        PrimaryEntity,
        [Display(Name = "Consumption")]
        Consumption,
        [Display(Name = "Occurrence")]
        Occurrence,
        [Display(Name = "Exposure")]
        Exposure,
        [Display(Name = "Hazard")]
        Hazard,
        [Display(Name = "Risks")]
        Risk,
        [Display(Name = "In-silico")]
        InSilico,
        [Display(Name = "Kinetic")]
        Kinetic
    }

    public enum ActionType {
        [Display(Name = "Action global settings")]
        Action = -2,
        [Display(Name = "Unknown")]
        Unknown = -1,
        [Display(Name = "Foods")]
        Foods = 0,
        [Display(Name = "Substances")]
        Substances = 1,
        [Display(Name = "Consumptions")]
        Consumptions = 2,
        [Display(Name = "Effects")]
        Effects = 3,
        [Display(Name = "Populations")]
        Populations = 4,
        [Display(Name = "Concentrations")]
        Concentrations = 5,
        [Display(Name = "Processing factors")]
        ProcessingFactors = 6,
        [Display(Name = "Unit variability factors")]
        UnitVariabilityFactors = 7,
        [Description("Obsolete: AgriculturalUses")]
        [Display(Name = "Occurrence patterns")]
        OccurrencePatterns = 8,
        [Display(Name = "Food conversions")]
        FoodConversions = 9,
        [Description("Obsolete: ConsumptionsPerFoodsAsMeasured")]
        [Display(Name = "Consumptions by modelled food")]
        ConsumptionsByModelledFood = 10,
        [Display(Name = "Concentration models")]
        ConcentrationModels = 11,
        [Display(Name = "Exposures")]
        TargetExposures = 12,
        [Display(Name = "Risks")]
        Risks = 13,
        [Description("Obsolete: TargetHazardDoses")]
        [Display(Name = "Hazard characterisations")]
        HazardCharacterisations = 14,
        [Display(Name = "Dietary exposures")]
        DietaryExposures = 15,
        [Display(Name = "Non-dietary exposures")]
        NonDietaryExposures = 16,
        [Description("Obsolete: HazardDoses")]
        [Display(Name = "Points of departure")]
        PointsOfDeparture = 17,
        [Display(Name = "Exposure mixtures")]
        ExposureMixtures = 18,
        [Display(Name = "Relative potency factors")]
        RelativePotencyFactors = 19,
        [Description("Obsolete: ResidueLimits")]
        [Display(Name = "Concentration limits")]
        ConcentrationLimits = 20,
        [Display(Name = "Inter-species conversions")]
        InterSpeciesConversions = 21,
        [Display(Name = "Test systems")]
        TestSystems = 22,
        [Description("Obsolete: AssessmentGroupMemberships")]
        [Display(Name = "Active substances")]
        ActiveSubstances = 24,
        [Display(Name = "Dose response models")]
        DoseResponseModels = 25,
        [Display(Name = "Dose response data")]
        DoseResponseData = 26,
        [Display(Name = "Responses")]
        Responses = 27,
        [Display(Name = "Effect representations")]
        EffectRepresentations = 28,
        [Display(Name = "AOP networks")]
        AOPNetworks = 30,
        [Display(Name = "Intra species factors")]
        IntraSpeciesFactors = 31,
        [Display(Name = "Kinetic models")]
        KineticModels = 32,
        [Description("Obsolete: FoodsAsMeasured")]
        [Display(Name = "Modelled foods")]
        ModelledFoods = 33,
        [Description("Obsolete: DietaryExposureScreening")]
        [Display(Name = "High exposure food-substance combinations")]
        HighExposureFoodSubstanceCombinations = 34,
        [Description("Obsolete: FocalFoods")]
        [Display(Name = "Focal food concentrations")]
        FocalFoodConcentrations = 35,
        [Display(Name = "Molecular docking models")]
        MolecularDockingModels = 36,
        [Display(Name = "QSAR membership models")]
        QsarMembershipModels = 37,
        [Display(Name = "Human monitoring data")]
        HumanMonitoringData = 38,
        [Display(Name = "Human monitoring analysis")]
        HumanMonitoringAnalysis = 39,
        [Description("Obsolete: ResidueDefinitions")]
        [Display(Name = "Substance conversions")]
        SubstanceConversions = 40,
        [Display(Name = "Food recipes")]
        FoodRecipes = 41,
        [Display(Name = "Total diet study sample compositions")]
        TotalDietStudyCompositions = 42,
        [Display(Name = "Food extrapolations")]
        FoodExtrapolations = 43,
        [Display(Name = "Market shares")]
        MarketShares = 44,
        [Description("Obsolete: AuthorisedUses")]
        [Display(Name = "Substance authorisations")]
        SubstanceAuthorisations = 45,
        [Display(Name = "Single value dietary exposures")]
        SingleValueDietaryExposures = 46,
        [Display(Name = "Single value consumptions")]
        SingleValueConsumptions = 47,
        [Display(Name = "Single value concentrations")]
        SingleValueConcentrations = 48,
        [Display(Name = "Deterministic substance conversion factors")]
        DeterministicSubstanceConversionFactors = 49,
        [Display(Name = "Occurrence frequencies")]
        OccurrenceFrequencies = 50,
        [Display(Name = "Single value risks")]
        SingleValueRisks = 51,
        [Display(Name = "Concentration distributions")]
        ConcentrationDistributions = 52,
        [Display(Name = "Biological matrix concentration comparisons")]
        BiologicalMatrixConcentrationComparisons = 53,
        [Display(Name = "Non-dietary exposure sources")]
        NonDietaryExposureSources = 54,
        [Display(Name = "Substance approvals")]
        SubstanceApprovals = 55,
        [Display(Name = "Exposure biomarker conversions")]
        ExposureBiomarkerConversions = 56,
        [Display(Name = "Single value non-dietary exposures")]
        SingleValueNonDietaryExposures = 57,
    }
}
