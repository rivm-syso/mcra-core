using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionPathStatisticsRecord {

        [DisplayName("Conversion path")]
        [Description("The sequence of conversion steps followed in the conversion")]
        public string ConversionPath { get; set; }

        [DisplayName("Total occurrences")]
        [Description("The total number of times that this conversion path is followed")]
        public int TotalOccurrences { get; set; }

    }
}
