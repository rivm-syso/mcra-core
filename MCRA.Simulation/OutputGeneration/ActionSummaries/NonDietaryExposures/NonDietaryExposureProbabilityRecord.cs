using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposureProbabilityRecord {

        [DisplayName("Code")]
        public string Code { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Percentage zeros (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Probability { get; set; }

        [DisplayName("Number of exposure sets")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int ExposureSets { get; set; }


    }
}
