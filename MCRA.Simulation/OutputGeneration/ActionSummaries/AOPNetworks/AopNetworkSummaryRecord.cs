using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AopNetworkSummaryRecord {

        [DisplayName("AOP code")]
        public string AOPCode { get; set; }

        [DisplayName("AOP name")]
        public string AOPName { get; set; }

        [DisplayName("Description")]
        public string AOPDescription { get; set; }

        [DisplayName("Risk type")]
        public string RiskType { get; set; }

        [DisplayName("Reference")]
        public string Reference { get; set; }

        [DisplayName("AOP Wiki Id")]
        public string AOPWikiIds { get; set; }

        [DisplayName("Adverse outcome name")]
        public string AdverseOutcomeName { get; set; }

        [DisplayName("Adverse outcome code")]
        public string AdverseOutcomeCode { get; set; }
    }
}
