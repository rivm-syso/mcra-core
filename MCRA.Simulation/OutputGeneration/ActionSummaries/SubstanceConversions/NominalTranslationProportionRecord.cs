using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NominalTranslationProportionRecord {

        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

        [DisplayName("Active substance names")]
        public List<string> ActiveSubstanceNames { get; set; }

        [DisplayName("Active substance codes")]
        public List<string> ActiveSubstanceCodes { get; set; }

        [DisplayName("Conversion factors")]
        public List<double> ConversionFactors { get; set; }

        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Proportion { get; set; }

    }
}
