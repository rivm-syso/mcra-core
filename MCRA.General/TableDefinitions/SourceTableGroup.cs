using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MCRA.General {

    /// <summary>
    /// Definitions of the SourceDataTable Groups that exist
    /// </summary>
    public enum SourceTableGroup {
        Unknown = -1,
        [Display(Name = "Foods", Order = 1)]
        Foods = 0,
        [Display(Name = "Substances", Order = 2)]
        Compounds = 6,
        [Display(Name = "Effects", Order = 3)]
        Effects = 2,
        [Display(Name = "Consumptions", Order = 4)]
        Survey = 1,
        [Display(Name = "Concentrations", Order = 5)]
        Concentrations = 3,
        [Display(Name = "Processing", Order = 7)]
        Processing = 4,
        [Display(Name = "Unit variability", Order = 8)]
        [XmlEnum("UnitVariability")]
        UnitVariabilityFactors = 5,
        [Display(Name = "Agricultural use", Order = 9)]
        AgriculturalUse = 7,
        [Display(Name = "Non-dietary", Order = 10)]
        NonDietary = 8,
        [Display(Name = "Focal food samples", Order = 11)]
        FocalFoods = 9,
        [Display(Name = "Concentration limits", Order = 12)]
        MaximumResidueLimits = 10,
        [Display(Name = "Total diet study", Order = 15)]
        TotalDietStudy = 11,
        [Display(Name = "Responses", Order = 16)]
        Responses = 17,
        [Display(Name = "Kinetic model data", Order = 17)]
        KineticModels = 13,
        [Display(Name = "Dose response data", Order = 18)]
        DoseResponseData = 12,
        [Display(Name = "Dose response models", Order = 19)]
        DoseResponseModels = 14,
        [Display(Name = "Effect representations", Order = 20)]
        EffectRepresentations = 15,
        [Display(Name = "Adverse outcome pathway networks", Order = 21)]
        AdverseOutcomePathwayNetworks = 16,
        [Display(Name = "Test systems", Order = 22)]
        TestSystems = 18,
        [Display(Name = "Relative potency factors", Order = 13)]
        RelativePotencyFactors = 19,
        [Display(Name = "Points of departure", Order = 14)]
        HazardDoses = 20,
        [Display(Name = "Active substances", Order = 23)]
        AssessmentGroupMemberships = 21,
        [Display(Name = "Inter system factors", Order = 24)]
        InterSpeciesFactors = 22,
        [Display(Name = "Intra species factors", Order = 25)]
        IntraSpeciesFactors = 23,
        [Display(Name = "Molecular docking models", Order = 26)]
        MolecularDockingModels = 24,
        [Display(Name = "QSAR membership models", Order = 27)]
        QsarMembershipModels = 25,
        [Display(Name = "Human biomonitoring data", Order = 28)]
        HumanMonitoringData = 26,
        [Display(Name = "Populations", Order = 29)]
        Populations = 27,
        [Display(Name = "Substance conversions", Order = 30)]
        ResidueDefinitions = 28,
        [Display(Name = "Food recipes", Order = 31)]
        FoodTranslations = 29,
        [Display(Name = "Food extrapolations", Order = 32)]
        FoodExtrapolations = 30,
        [Display(Name = "Market shares", Order = 33)]
        MarketShares = 31,
        [Display(Name = "Authorised uses", Order = 34)]
        AuthorisedUses = 32,
        [Display(Name = "Dietary exposures", Order = 35)]
        DietaryExposures = 33,
        [Display(Name = "Hazard characterisations", Order = 37)]
        HazardCharacterisations = 35,
        [Display(Name = "Target exposures", Order = 36)]
        TargetExposures = 34,
        [Display(Name = "Risks", Order = 38)]
        Risks = 36,
        [Display(Name = "Single value consumptions", Order = 39)]
        SingleValueConsumptions = 37,
        [Display(Name = "Single value concentrations", Order = 40)]
        SingleValueConcentrations = 38,
        [Display(Name = "Deterministic substance conversion factors", Order = 41)]
        DeterministicSubstanceConversionFactors = 39,
        [Display(Name = "Occurrence frequencies", Order = 42)]
        OccurrenceFrequencies = 40,
        [Display(Name = "Concentration distributions", Order = 43)]
        ConcentrationDistributions = 41,
        [Display(Name = "Non-dietary exposure sources", Order = 44)]
        NonDietaryExposureSources = 42,
        [Display(Name = "Substance approvals", Order = 45)]
        SubstanceApprovals = 43,
        [Display(Name = "Exposure  biomarker conversion factors", Order = 46)]
        ExposureBiomarkerConversions = 44,
        [Display(Name = "Single value non-dietary exposures", Order = 47)]
        SingleValueNonDietaryExposures = 45,
        [Display(Name = "PBK model parametrisations", Order = 48)]
        PbkModels = 46,
        [Display(Name = "Kinetic conversion factors", Order = 49)]
        KineticConversionFactors = 47,
        [Display(Name = "Dust concentration distributions", Order = 50)]
        DustConcentrationDistributions = 48,
        [Display(Name = "Dust exposure determinants", Order = 51)]
        DustExposureDeterminants = 49,
        [Display(Name = "Exposure response functions", Order = 52)]
        ExposureResponseFunctions = 50,
        [Display(Name = "PBK model definitions", Order = 53)]
        PbkModelDefinitions = 51,
        [Display(Name = "Soil concentration distributions", Order = 54)]
        SoilConcentrationDistributions = 52,
        [Display(Name = "Soil exposure determinants", Order = 55)]
        SoilExposureDeterminants = 53,
        [Display(Name = "Burden of disease", Order = 56)]
        BurdensOfDisease = 54,
        [Display(Name = "Indoor air concentrations", Order = 57)]
        IndoorAirConcentrations = 55,
        [Display(Name = "Air exposure determinants", Order = 58)]
        AirExposureDeterminants = 56,
        [Display(Name = "Outdoor air concentrations", Order = 59)]
        OutdoorAirConcentrations = 57,
        [Display(Name = "Consumer products", Order = 60)]
        ConsumerProducts = 58,
        [Display(Name = "Consumer product use frequencies", Order = 61)]
        ConsumerProductUseFrequencies = 59,
        [Display(Name = "Consumer product concentrations", Order = 62)]
        ConsumerProductConcentrations = 60,
        [Display(Name = "Consumer product exposure determinants", Order = 63)]
        ConsumerProductExposureDeterminants = 61,
        [Display(Name = "Consumer product concentration distributions", Order = 64)]
        ConsumerProductConcentrationDistributions = 62,
        [Display(Name = "HBM single value exposures", Order = 65)]
        HbmSingleValueExposures = 63,
    }
}
