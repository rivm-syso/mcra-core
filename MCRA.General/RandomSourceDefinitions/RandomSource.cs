namespace MCRA.General {
    public enum RandomSource {
        // Hazard characterisations: draw kinetic model parameters
        HC_DrawKineticModelParameters = 1,
        // Biological matrix exposures: draw kinetic model parameters
        BME_DrawKineticModelParameters = 2,
        // Biological matrix exposures: draw non-dietary exposures
        BME_DrawNonDietaryExposures = 3,
        // Dietary exposures: draw individuals
        DE_DrawIndividuals = 4,
        // Dietary exposures: draw unit variability factor
        DE_DrawUnitVariabilityFactor = 5,
        // Dietary exposures: draw processing factor
        DE_DrawProcessingFactor = 6,
        // Dietary exposures: draw market share
        DE_DrawMarketShare = 7,
        // Dietary exposures: draw consumption amount
        DE_DrawConsumptionAmount = 8,
        // Dietary exposures: draw unit weight
        DE_DrawUnitWeight = 9,
        // Dietary exposures: draw concentration
        DE_DrawConcentration = 10,
        // Dietary exposures: draw imputed exposures
        DE_DrawImputedExposures = 11,
        // Dietary exposures: draw model-based exposures
        DE_DrawModelBasedExposures = 12,
        // Dietary exposures: draw model-assisted exposures
        DE_DrawModelAssistedExposures = 14,
        // Risks: draw inta-species factors
        RSK_DrawIntraSpeciesFactors = 15,
        // Concentrations: active substance allocation
        CONC_RandomActiveSubstanceAllocation = 16,
        // Concentrations: active substance allocation
        CONC_RandomConcentrationExtrapolation = 17,
        // Concentrations: active substance allocation
        CONC_RandomFocalConcentrationReplacement = 18,
        // Concentration modelling: non-detects imputation
        CM_NonDetectsImputation = 20,
        // Concentration modelling: missing value imputation
        CM_MissingValueImputation = 21,
        // Human monitoring: missing value imputation
        HBM_MissingValueImputation = 19,
        // Mixtures calculation: mixture NMF calculation initialisation
        MIX_NmfInitialisation = 22,
        // Human monitoring: censored value imputation
        HBM_CensoredValueImputation = 23,
        // Human monitoring: exposure biomarker conversion variability
        HBM_ExposureBiomarkerConversion = 24,
        // Human monitoring: kinetic model conversion 
        HBM_KineticConversionFactor = 25,
        // Dust exposures: draw individuals
        DUE_DrawIndividuals = 26,
        // Dust exposures: draw dust exposures
        DUE_DrawDustExposures = 27,
    }
}
