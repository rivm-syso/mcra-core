using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class EnvironmentalBurdenOfDiseaseSummaryRecord {

        [Description("Burden of disease indicator.")]
        [DisplayName("BoD indicator")]
        public string BodIndicator { get; set; }

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF code")]
        public string ErfCode { get; set; }

        [Description("The name of the exposure response function.")]
        [DisplayName("ERF name")]
        public string ErfName { get; set; }

        [Description("Total attributable burden of disease for the whole population.")]
        [DisplayName("Total attributable BoD")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double TotalAttributableBod { get; set; }
    }
}
