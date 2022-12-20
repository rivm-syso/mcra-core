using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelSubstanceRecord {

        [Description("compound name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("compound code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("membership score")]
        [DisplayName("Membership score")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MembershipScore { get; set; }

    }
}
