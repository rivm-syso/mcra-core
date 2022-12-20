using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EffectRelationshipSummaryRecord {

        [DisplayName("Effect code")]
        public string EffectCode { get; set; }

        [DisplayName("Effect name")]
        public string EffectName { get; set; }

        [DisplayName("Biological organization")]
        public string BiologicalOrganisation { get; set; }

        [DisplayName("AOP Wiki Ids")]
        public string AOPWikiIds { get; set; }

        [Display(AutoGenerateField = false)]
        public int OrderNumber { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsSelectedEffect { get; set; }

        [Display(AutoGenerateField = false)]
        public bool AdverseOutcome { get; set; }

        [Display(AutoGenerateField = false)]
        public List<string> UpstreamKeyEventCodes { get; set; }

        [DisplayName("Upstream key events")]
        public string UpstreamKeyEvents {
            get {
                return string.Join(", ", UpstreamKeyEventCodes);
            }
        }

        [Display(AutoGenerateField = false)]
        public List<string> DownstreamKeyEventCodes { get; set; }

        [DisplayName("Downstream key events")]
        public string DownstreamKeyEvents {
            get {
                return string.Join(", ", DownstreamKeyEventCodes);
            }
        }
    }
}
