using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstanceModelRecord {

        [Description("The code of the assessment group.")]
        [DisplayName("Code assessment group")]
        public string Code { get; set; }

        [Description("The name of the assessment group.")]
        [DisplayName("CAG name")]
        public string Name { get; set; }

        [Display(AutoGenerateField = false)]
        [Description("Description of the assessment group.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("The code of the effect to which the memberships apply.")]
        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [Description("The name of the effect to which the memberships apply.")]
        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [Description("Reference.")]
        [DisplayName("Reference")]
        public string Reference { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsProbabilistic { get; set; }

        [Description("Number of computed membership scores.")]
        [DisplayName("Number of computed membership scores")]
        public int MembershipScoresCount { get; set; }

        [Description("Number of missing membership scores.")]
        [DisplayName("Number of missing membership scores")]
        public int MissingMembershipScoresCount { get; set; }

        [Description("Number of possible memberships.")]
        [DisplayName("Number of possible memberships")]
        public int ProbableMembershipsCount { get; set; }

        [Description("Number of certain memberships.")]
        [DisplayName("Number of certain memberships")]
        public int CertainMembershipsCount { get; set; }

        [Description("Mean membership score.")]
        [DisplayName("Mean of computed membership scores")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MembershipScoresMean { get; set; }

        [Description("Median membership score.")]
        [DisplayName("Median of computed membership scores")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MembershipScoresMedian { get; set; }

        [Description("Fraction of possible membership scores.")]
        [DisplayName("Fraction possible membership")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FractionProbableMemberships {
            get {
                return MembershipScoresCount > 0 ? (double)ProbableMembershipsCount / MembershipScoresCount : double.NaN;
            }
        }

        [Description("Fraction of certain membership scores.")]
        [DisplayName("Fraction certain membership")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FractionCertainMemberships {
            get {
                return MembershipScoresCount > 0 ? (double)CertainMembershipsCount / MembershipScoresCount : double.NaN;
            }
        }

        [Display(AutoGenerateField = false)]
        public List<ActiveSubstanceRecord> MembershipProbabilities { get; set; }
    }
}
