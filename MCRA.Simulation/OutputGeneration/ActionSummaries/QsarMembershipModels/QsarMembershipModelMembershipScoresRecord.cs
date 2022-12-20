using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelMembershipScoresRecord {

        [Description("QSAR model code")]
        [DisplayName("Model code")]
        public string Code { get; set; }

        [Description("QSAR model name")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Display(AutoGenerateField = false)]
        public List<QsarMembershipModelSubstanceRecord> MembershipScores { get; set; }

    }
}
