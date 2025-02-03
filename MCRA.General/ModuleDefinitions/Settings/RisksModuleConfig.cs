using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class RisksModuleConfig {
        [XmlIgnore]
        public double[] RiskPercentiles {
            get {
                return RiskMetricType == RiskMetricType.HazardExposureRatio
                    ? SelectedPercentiles
                        .Select(c => 100 - c).Reverse().ToArray()
                    : SelectedPercentiles.ToArray();
            }
        }

        [XmlIgnore]
        public bool IsCumulative => MultipleSubstances && CumulativeRisk;
    }
}
