using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class SingleValueRisksModuleConfig {
        [XmlIgnore]
        public bool IsFocalCommodityMeasurementReplacement =>
               FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
            || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
            || FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval;
    }
}
