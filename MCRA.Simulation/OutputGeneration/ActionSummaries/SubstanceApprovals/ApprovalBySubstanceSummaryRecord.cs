using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for approvals by substance.
    /// </summary>
    public sealed class ApprovalBySubstanceSummaryRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Approval status of the substance.")]
        [DisplayName("Approved")]
        public bool IsApproved { get; set; }
    }
}
