using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summary record for consumption frequencies.
    /// </summary>
    public sealed class DateTimeMonthRecord {

        [DisplayName("Months")]
        public string Month{ get; set; }

        [DisplayName("Number of days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [DisplayName("Percentage (%)")]
        [DisplayFormat(DataFormatString = "{0:F0}")]
        public double Percentage { get; set; }

    }
}
