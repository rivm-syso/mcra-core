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
        [Display(Name = "Risk")]
        Risk,
        [Display(Name = "In-silico")]
        InSilico,
        [Display(Name = "Kinetic")]
        Kinetic
    }

    public enum ActionType {
        [Display(Name = "PBK model simulations")]
        PbkModelSimulations = -3,
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
        [Description("Obsolete: TargetExposures")]
        [Display(Name = "Internal exposures")]
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
        [Description("Obsolete: KineticModels")]
        [Display(Name = "Absorption factors")]
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
        [Description("Obsolete: HumanMonitoringData")]
        [Display(Name = "HBM concentrations")]
        HumanMonitoringData = 38,
        [Description("Obsolete: HumanMonitoringAnalysis")]
        [Display(Name = "HBM analysis")]
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
        [Description("Obsolete: PbkModels")]
        [Display(Name = "PBK model parametrisations")]
        PbkModels = 58,
        [Display(Name = "Kinetic conversion factors")]
        KineticConversionFactors = 59,
        [Display(Name = "Dust concentrations")]
        DustConcentrations = 60,
        [Display(Name = "Dust exposure determinants")]
        DustExposureDeterminants = 61,
        [Display(Name = "Dust exposures")]
        DustExposures = 62,
        [Display(Name = "Environmental burden of disease")]
        EnvironmentalBurdenOfDisease = 63,
        [Display(Name = "Exposure response functions")]
        ExposureResponseFunctions = 64,
        [Display(Name = "PBK model definitions")]
        PbkModelDefinitions = 65,
        [Display(Name = "Soil concentrations")]
        SoilConcentrations = 66,
        [Display(Name = "Soil exposure determinants")]
        SoilExposureDeterminants = 67,
        [Display(Name = "Soil exposures")]
        SoilExposures = 68,
        [Display(Name = "Burden of disease")]
        BurdensOfDisease = 69,
        [Display(Name = "Indoor air concentrations")]
        IndoorAirConcentrations = 70,
        [Display(Name = "Air exposures")]
        AirExposures = 71,
        [Display(Name = "Air exposure determinants")]
        AirExposureDeterminants = 72,
        [Display(Name = "Outdoor air concentrations")]
        OutdoorAirConcentrations = 73,
        [Display(Name = "Individuals")]
        Individuals = 74,
        [Display(Name = "Consumer products")]
        ConsumerProducts = 75,
        [Display(Name = "Consumer product use frequencies")]
        ConsumerProductUseFrequencies = 76,
        [Display(Name = "Consumer product concentrations")]
        ConsumerProductConcentrations = 77,
        [Display(Name = "Consumer product exposure determinants")]
        ConsumerProductExposureDeterminants = 78,
        [Display(Name = "Consumer product exposures")]
        ConsumerProductExposures = 79,
        [Display(Name = "Consumer product concentration distributions")]
        ConsumerProductConcentrationDistributions = 80,
        [Display(Name = "HBM single value exposures")]
        HbmSingleValueExposures = 81,
        [Display(Name = "Dust concentration distributions")]
        DustConcentrationDistributions = 82,
        [Display(Name = "Occupational task exposures")]
        OccupationalTaskExposures = 83,
        [Display(Name = "Occupational exposures")]
        OccupationalExposures = 84,
        [Display(Name = "Occupational scenarios")]
        OccupationalScenarios = 85,
    }
}
