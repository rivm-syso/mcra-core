using System.Xml.Serialization;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class ConcentrationsModuleConfig :
        IFocalCommodityMeasurementReplacementCalculatorFactorySettings,
        IActiveSubstanceAllocationSettings,
        IFoodExtrapolationCandidatesCalculatorSettings,
        IWaterConcentrationsExtrapolationCalculatorSettings {

        [XmlIgnore]
        public bool IsFocalCommodityMeasurementReplacement =>
            FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue;

        [XmlIgnore]
        public bool IsSamplePropertySubset => SampleSubsetSelection;

        [XmlIgnore]
        public SamplesSubsetDefinition RegionSubsetDefinition =>
            SamplesSubsetDefinitions.FirstOrDefault(r => r.IsRegionSubset);

        [XmlIgnore]
        public SamplesSubsetDefinition ProductionMethodSubsetDefinition =>
            SamplesSubsetDefinitions.FirstOrDefault(r => r.IsProductionMethodSubset);

        [XmlIgnore]
        public List<SamplesSubsetDefinition> AdditionalSamplePropertySubsetDefinitions =>
            SamplesSubsetDefinitions
                .Where(r => !r.IsRegionSubset && !r.IsProductionMethodSubset)
                .ToList();

        [XmlIgnore]
        public SubstanceTranslationAllocationMethod ReplacementMethod => SubstanceTranslationAllocationMethod;

        [XmlIgnore]
        public bool UseSubstanceAuthorisations => ConsiderAuthorisationsForSubstanceConversion;
    }
}
