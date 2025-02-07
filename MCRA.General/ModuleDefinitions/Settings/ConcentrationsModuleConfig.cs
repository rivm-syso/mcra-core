using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class ConcentrationsModuleConfig {
        [XmlIgnore]
        public bool IsFocalCommodityMeasurementReplacement =>
            FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval
                || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue;
    }
}
