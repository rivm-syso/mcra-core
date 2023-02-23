using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class EffectRelationshipsSummarySection : SummarySection {

        public List<EffectRelationshipSummaryRecord> Records { get; set; }

        [DisplayName("Name AOP network")]
        public string AdverseOutcomePathwayNetworkName { get; set; }

        [DisplayName("Name adverse outcome")]
        public string AdverseOutcomeName { get; set; }

        [DisplayName("Code adverse outcome")]
        public string AdverseOutcomeCode { get; set; }

        [DisplayName("Incomplete effect relationship definition set")]
        public bool IncompleteEffectRelationships { get; set; }

        public void Summarize(AdverseOutcomePathwayNetwork aopNetwork, ICollection<Effect> relevantEffects) {
            var selectedEffectRelations = aopNetwork.EffectRelations
                .Where(r => relevantEffects.Contains(r.UpstreamKeyEvent) && relevantEffects.Contains(r.DownstreamKeyEvent))
                .ToList();
            var adverseOutcome = aopNetwork.AdverseOutcome;
            AdverseOutcomePathwayNetworkName = aopNetwork.Name;
            AdverseOutcomeName = aopNetwork.AdverseOutcome.Name;
            AdverseOutcomeCode = aopNetwork.AdverseOutcome.Code;

            var relationshipRecords = selectedEffectRelations;

            var effectOrderNumbers = new Dictionary<Effect, int>();
            effectOrderNumbers.Add(adverseOutcome, 0);
            var count = 0;
            if (relationshipRecords != null) {
                var stack = new Stack<Effect>();
                stack.Push(adverseOutcome);
                while (stack.Count > 0) {
                    var currentEffect = stack.Pop();
                    var upstreamEvents = relationshipRecords
                        .Where(r => r.DownstreamKeyEvent == currentEffect)
                        .Select(r => r.UpstreamKeyEvent);
                    foreach (var upstreamEvent in upstreamEvents) {
                        if (!effectOrderNumbers.ContainsKey(upstreamEvent)) {
                            stack.Push(upstreamEvent);
                            effectOrderNumbers.Add(upstreamEvent, ++count);
                        }
                    }
                }
            }
            IncompleteEffectRelationships = !relevantEffects.All(r => effectOrderNumbers.ContainsKey(r));

            var records = new List<EffectRelationshipSummaryRecord>();
            var percentages = new double[] { 25, 50, 75 };
            foreach (var effect in relevantEffects) {
                var record = new EffectRelationshipSummaryRecord() {
                    EffectCode = effect.Code,
                    EffectName = effect.Name,
                    OrderNumber = effectOrderNumbers.ContainsKey(effect) ? effectOrderNumbers[effect] : count++,
                    IsSelectedEffect = effect == aopNetwork.AdverseOutcome,
                    AdverseOutcome = effect == adverseOutcome,
                    BiologicalOrganisation = effect.BiologicalOrganisationType.GetDisplayName(false),
                    UpstreamKeyEventCodes = relationshipRecords?.Where(r => r.DownstreamKeyEvent == effect).Select(r => r.UpstreamKeyEvent.Code).ToList(),
                    DownstreamKeyEventCodes = relationshipRecords?.Where(r => r.UpstreamKeyEvent == effect).Select(r => r.DownstreamKeyEvent.Code).ToList(),
                    AOPWikiIds = effect.AOPWikiIds
                };
                records.Add(record);
            }
            Records = records.OrderBy(r => r.OrderNumber).ToList();
        }
    }
}
