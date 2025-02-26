using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class EnvironmentalBurdenOfDiseaseSummaryRecord {

        [Description("Total attributable burden of disease for the whole population.")]
        [DisplayName("Total attributable burden of disease")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double TotalAttributableBod { get; set; }
    }
}
