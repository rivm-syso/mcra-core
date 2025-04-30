using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum UncertaintySource {
        [Display(Name = "Relative potency factors")]
        RPFs = 1,
        [Display(Name = "Assessment group memberships")]
        AssessmentGroupMemberships = 2,
        // Consumptions: sampling uncertainty
        [Display(Name = "Individuals sample")]
        Individuals = 3,
        // Concentrations: sampling uncertainty
        [Display(Name = "Concentration samples")]
        Concentrations = 4,
        [Display(Name = "Consumption portions")]
        Portions = 5,
        [Display(Name = "Processing factors")]
        Processing = 6,
        [Display(Name = "Inter-species factors")]
        InterSpecies = 7,
        [Display(Name = "Intra-species factors")]
        IntraSpecies = 8,
        ParameterValues = 9,
        // Dose-response models: dose response model uncertainty
        [Display(Name = "Dose respone models")]
        DoseResponseModels = 10,
        NonDietaryExposures = 11,
        ImputeExposureDistributions = 12,
        [Display(Name = "Kinetic conversion factors")]
        KineticConversionFactors = 13,
        [Display(Name = "Active substance allocation")]
        ActiveSubstanceAllocation = 14,
        [Display(Name = "Concentration extrapolation")]
        ConcentrationExtrapolation = 15,
        [Display(Name = "Focal commodity sample/measurement replacement")]
        FocalCommodityReplacement = 16,
        [Display(Name = "Hazard characterisations selection")]
        HazardCharacterisationsSelection = 17,
        [Display(Name = "Hazard characterisations imputation")]
        HazardCharacterisationsImputation = 18,
        [Display(Name = "Points of departure")]
        PointsOfDeparture = 19,
        [Display(Name = "Single value risk adjustment factors")]
        SingleValueRiskAdjustmentFactors = 20,
        [Display(Name = "Concentration models")]
        ConcentrationModelling = 21,
        [Display(Name = "Missing concentration value imputation")]
        ConcentrationMissingValueImputation = 22,
        [Display(Name = "Non-detect concentration imputation")]
        ConcentrationNonDetectImputation = 23,
        [Display(Name = "HBM individuals sample")]
        HbmIndividuals = 24,
        [Display(Name = "HBM Non-detect concentration imputation")]
        HbmNonDetectImputation = 25,
        [Display(Name = "HBM Missing value imputation")]
        HbmMissingValueImputation = 26,
        [Display(Name = "Hazard characterisations")]
        HazardCharacterisations = 27,
        [Display(Name = "Exposure biomarker conversion")]
        ExposureBiomarkerConversion = 28,
        [Display(Name = "PBK model parameters")]
        PbkModelParameters = 29,
        [Display(Name = "Dust exposures")]
        DustExposures = 30,
        [Display(Name = "Exposure response functions")]
        ExposureResponseFunctions = 31,
    }
}
