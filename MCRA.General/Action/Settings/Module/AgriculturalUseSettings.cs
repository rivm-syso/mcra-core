namespace MCRA.General.Action.Settings {
    public class AgriculturalUseSettings {

        public virtual SettingsTemplateType OccurrencePatternsTier { get; set; }

        public virtual bool UseAgriculturalUseTable { get; set; }

        public virtual bool SetMissingAgriculturalUseAsUnauthorized { get; set; }

        public virtual bool UseAgriculturalUsePercentage { get; set; }

        public virtual bool ScaleUpOccurencePatterns { get; set; }

        public virtual bool RestrictOccurencePatternScalingToAuthorisedUses { get; set; }

        public virtual bool UseOccurrenceFrequencies { get; set; }

        public virtual bool UseOccurrencePatternsForResidueGeneration { get; set; }
    }
}
