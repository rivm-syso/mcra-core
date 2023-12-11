using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class HCSubgroupFromDataSummaryRecord : HazardCharacterisationsSummaryRecordBase {

        [Description("Number of subgroups.")]
        [Display(Name = "Number of subgroups", Order = 100)]
        public int? NumberOfSubgroups { get; set; }

        [Description("Number of subgroups with uncertainty sets.")]
        [Display(Name = "Subgroups with uncertainty sets", Order = 100)]
        public int? NumberOfSubgroupsWithUncertainty { get; set; }

        [Description("Total number of uncertainty sets.")]
        [Display(Name = "Total number of uncertainty sets", Order = 100)]
        public int? TotalNumberOfUncertaintySets { get; set; }

        [Description("Minimum number of uncertainty sets in a subgroup.")]
        [Display(Name = "Minimum number of uncertainty sets in subgroup", Order = 100)]
        public int? MinimumNumberUncertaintySets { get; set; }

        [Description("Maximum number of uncertainty sets in a subgroup.")]
        [Display(Name = "Maximum number of uncertainty sets in subgroup", Order = 100)]
        public int? MaximumNumberUncertaintySets { get; set; }
    }
}
