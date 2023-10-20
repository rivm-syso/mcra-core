using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardCharacterisationsFromDataSummaryRecord : HazardCharacterisationsSummaryRecordBase {

        [Description("Number of uncertainty sets.")]
        [Display(Name = "Uncertainty sets (N)", Order = 100)]
        public int? NumberOfUncertaintySets { get; set; }

        [Description("Median point of hazard uncertainty sets.")]
        [Display(Name = "Unc median", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double Median { get; set; }

        [Description("Minimum point of hazard uncertainty sets.")]
        [Display(Name = "Unc min", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double Minimum { get; set; }

        [Description("Maximum point of hazard uncertainty sets.")]
        [Display(Name = "Unc max", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G6}")]
        public double Maximum { get; set; }

    }
}
