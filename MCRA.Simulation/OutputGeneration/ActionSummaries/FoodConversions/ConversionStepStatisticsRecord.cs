using System.ComponentModel;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionStepStatisticsRecord {

        [DisplayName("Conversion step")]
        public FoodConversionStepType Step { get; set; }

        [Description("The number of conversion paths with this step")]
        [DisplayName("Number of paths with step")]
        public int PathsWithStep { get; set; }

        [DisplayName("Total number of times applied")]
        [Description("The total number of times this step is used (a step can occur multiple times in a conversion path)")]
        public int TotalOccurrences { get; set; }
    }
}
