using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summary record for consumption frequencies.
    /// </summary>
    public sealed class ConsumptionFrequencyRecord {

        [DisplayName("Number of days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [DisplayName("Number of individuals")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividuals { get; set; }

        [DisplayName("Percentage of individuals (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageOfAllIndividuals { get; set; }

    }
}
