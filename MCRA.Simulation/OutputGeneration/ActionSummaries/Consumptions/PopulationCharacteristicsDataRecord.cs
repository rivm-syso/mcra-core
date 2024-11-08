using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class PopulationCharacteristicsDataRecord {

        [DisplayName("Property")]
        [Description("Individual property.")]
        public string Property { get; set; }

        [DisplayName("Labels")]
        [Description("Distinct levels.")]
        public string Labels {
            get {
                if (Levels?.Count > 0) {
                    return string.Join(", ", Levels.Select(r => $"{r.Level} (#{r.Frequency:F0})"));
                } else {
                    return $"{DistinctValues} distinct values";
                }
            }
        }

        [Display(AutoGenerateField = false)]
        public List<PopulationLevelStatisticRecord> Levels { get; set; }

        [DisplayName("Mean")]
        [Description("Mean value value of the selected individuals (weighted by sampling weight when sampling weights are used).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Mean { get; set; }

        [DisplayName("p25")]
        [Description("Lower p25 value of the selected individuals (weighted by sampling weight when sampling weights are used).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? P25 { get; set; }

        [DisplayName("Median")]
        [Description("Median value of the selected individuals (weighted by sampling weight when sampling weights are used).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Median { get; set; }

        [DisplayName("p75")]
        [Description("p75 percentile value of the selected individuals (weighted by sampling weight when sampling weights are used).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? P75 { get; set; }

        [DisplayName("Min")]
        [Description("Lowest value of the selected individuals.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Min { get; set; }

        [DisplayName("Max")]
        [Description("Highest value of the selected individuals.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Max { get; set; }

        [DisplayName("Missing")]
        [Description("Total of individuals for which this attribute is missing/unknown (weighted by sampling weight when sampling weights are used).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Missing { get; set; }

        [Display(AutoGenerateField = false)]
        public double DistinctValues { get; set; }

    }
}
