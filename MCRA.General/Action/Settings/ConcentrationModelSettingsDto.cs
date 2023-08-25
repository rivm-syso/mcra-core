using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    public class ConcentrationModelSettingsDto {

        public virtual SettingsTemplateType ConcentrationsTier { get; set; }

        public virtual SettingsTemplateType ConcentrationModelChoice { get; set; } = SettingsTemplateType.Custom;

        public virtual ConcentrationModelType DefaultConcentrationModel { get; set; } = ConcentrationModelType.Empirical;

        public virtual bool IsFallbackMrl { get; set; }

        public virtual double FractionOfMrl { get; set; } = 1D;

        public virtual NonDetectsHandlingMethod NonDetectsHandlingMethod { get; set; }

        public virtual double FractionOfLOR { get; set; } = 0.5D;

        public virtual bool IsSampleBased { get; set; }

        public virtual bool ImputeMissingValues { get; set; }

        public virtual bool CorrelateImputedValueWithSamplePotency { get; set; }

        public virtual bool IsSingleSamplePerDay { get; set; }

        public virtual bool IsCorrelation { get; set; }

        public virtual bool UseComplexResidueDefinitions { get; set; }

        public virtual bool UseDeterministicConversionFactors { get; set; }

        public virtual SubstanceTranslationAllocationMethod SubstanceTranslationAllocationMethod { get; set; }

        public virtual bool ConsiderAuthorisationsForSubstanceConversion { get; set; }

        public virtual bool RetainAllAllocatedSubstancesAfterAllocation { get; set; }

        public virtual bool TryFixDuplicateAllocationInconsistencies { get; set; }

        public virtual List<ConcentrationModelTypePerFoodCompoundDto> ConcentrationModelTypesPerFoodCompound { get; set; } = new();

        public virtual bool ExtrapolateConcentrations { get; set; }

        public virtual int ThresholdForExtrapolation { get; set; } = 10;

        public virtual bool ConsiderMrlForExtrapolations { get; set; }

        public virtual bool ConsiderAuthorisationsForExtrapolations { get; set; }

        public virtual bool ImputeWaterConcentrations { get; set; }

        public virtual double WaterConcentrationValue { get; set; } = 0.05D;

        public virtual string CodeWater { get; set; }

        public virtual bool RestrictWaterImputationToAuthorisedUses { get; set; }

        public virtual bool RestrictWaterImputationToMostPotentSubstances { get; set; } = true;

        public virtual bool RestrictLorImputationToAuthorisedUses { get; set; }

        public virtual bool RestrictWaterImputationToApprovedSubstances { get; set; }

        public virtual bool FilterConcentrationLimitExceedingSamples { get; set; } = false;

        public virtual double ConcentrationLimitFilterFractionExceedanceThreshold { get; set; } = 1D;

        public virtual FocalCommodityReplacementMethod FocalCommodityReplacementMethod { get; set; }

        public virtual double FocalCommodityScenarioOccurrencePercentage { get; set; } = 100D;

        public virtual bool UseDeterministicSubstanceConversionsForFocalCommodity { get; set; } = false;

        public virtual double FocalCommodityConcentrationAdjustmentFactor { get; set; } = 1D;

        public virtual bool IsProcessing { get; set; }

        public virtual bool IsDistribution { get; set; }

        public virtual bool AllowHigherThanOne { get; set; }

        [XmlIgnore]
        public bool IsFocalCommodityMeasurementReplacement =>
               FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
            || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
            || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval;

    }
}
