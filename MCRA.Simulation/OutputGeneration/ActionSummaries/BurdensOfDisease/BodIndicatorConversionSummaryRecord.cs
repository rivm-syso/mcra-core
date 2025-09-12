using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MCRA.Simulation.OutputGeneration {
    public class BodIndicatorConversionSummaryRecord {

        [Description("Source indicator.")]
        [DisplayName("From indicator")]
        public string FromIndicator { get; set; }

        [Description("From unit.")]
        [DisplayName("From unit")]
        public string FromUnit { get; set; }

        [Description("To indicator.")]
        [DisplayName("To indicator")]
        public string ToIndicator { get; set; }

        [Description("To unit.")]
        [DisplayName("To unit")]
        public string ToUnit { get; set; }

        [Description("Conversion factor")]
        [DisplayName("Conversion factor")]
        public double Value { get; set; }
    }
}

