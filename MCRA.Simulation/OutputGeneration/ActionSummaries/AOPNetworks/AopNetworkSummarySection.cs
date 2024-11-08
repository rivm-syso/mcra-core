using MCRA.Data.Compiled.ObjectExtensions;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AopNetworkSummarySection : SummarySection {

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

        [Display(AutoGenerateField = false)]
        public List<AopKeyEventRecord> KeyEvents { get; set; }

        [Display(AutoGenerateField = false)]
        public List<AopKeyEventRelationshipRecord> KeyEventRelationships { get; set; }

        public void Summarize(AdverseOutcomePathwayNetwork aopNetwork, ICollection<Effect> relevantEffects) {
            AOPCode = aopNetwork.Code;
            AOPName = aopNetwork.Name;
            Reference = aopNetwork.Reference;
            AOPDescription = aopNetwork.Description;
            RiskType = aopNetwork.RiskTypeString;
            AOPWikiIds = aopNetwork.AdverseOutcome.AOPWikiIds;
            AdverseOutcomeCode = aopNetwork.AdverseOutcome.Code;
            AdverseOutcomeName = aopNetwork.AdverseOutcome.Name;

            var cyclicKers = aopNetwork.FindFeedbackRelationships().ToHashSet();
            var indirectKers = aopNetwork.GetIndirectKeyEventRelationships().ToHashSet();

            KeyEventRelationships = aopNetwork.EffectRelations
                .Where(r => relevantEffects.Contains(r.UpstreamKeyEvent)
                    && relevantEffects.Contains(r.DownstreamKeyEvent)
                )
                .Select(r => new AopKeyEventRelationshipRecord() {
                    CodeDownstreamKeyEvent = r.DownstreamKeyEvent.Code,
                    CodeUpstreamKeyEvent = r.UpstreamKeyEvent.Code,
                    IsCyclic = cyclicKers.Contains(r),
                    IsIndirect = indirectKers.Contains(r)
                })
                .ToList();

            KeyEvents = relevantEffects
                .Select(r => new AopKeyEventRecord() {
                    Code = r.Code,
                    Name = r.Name,
                    BiologicalOrganisationType = r.BiologicalOrganisationType,
                })
                .ToList();

        }
    }
}
