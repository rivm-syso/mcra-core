using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicSingleValueEstimatesRecord {

        [Display(Name = "Substance name", Order = 3)]
        public string SubstanceName { get; set; }

        [Display(Name = "Substance code", Order = 4)]
        public string SubstanceCode { get; set; }

        [Description("Exposure estimate for the population.")]
        [Display(Name = "Exposure (ExposureUnit)", Order = 30)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Display(Name = "Method", Order = 40)]
        public string CalculationMethod { get; set; }
    }
}

