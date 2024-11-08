using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.ObjectExtensions {
    public static class AdverseOutcomePathwayNetworkExtensions {

        /// <summary>
        /// Gets all effects of the AOP network.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static HashSet<Effect> GetAllEffects(this AdverseOutcomePathwayNetwork aopNetwork) {
            var result = aopNetwork.EffectRelations.Select(r => r.DownstreamKeyEvent)
                .Concat(aopNetwork.EffectRelations.Select(r => r.UpstreamKeyEvent))
                .ToHashSet();
            if (!result.Contains(aopNetwork.AdverseOutcome)) {
                result.Add(aopNetwork.AdverseOutcome);
            }
            return result;
        }

        /// <summary>
        /// Gets all effects of the sub-network of the AOP network defined
        /// by the effect specified as adverse outcome and the common upstream
        /// key event that should be shared by all paths of the sub-network.
        /// I.e., the key event relationships that are not part of a path
        /// (to the adverse outcome) that includes the common effect should
        /// be excluded.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <param name="adverseOutcome"></param>
        /// <param name="commonKeyEvent"></param>
        /// <returns></returns>
        public static AdverseOutcomePathwayNetwork GetSubNetwork(
            this AdverseOutcomePathwayNetwork aopNetwork,
            Effect adverseOutcome,
            Effect commonKeyEvent
        ) {
            if (commonKeyEvent != null) {
                var result = new HashSet<EffectRelationship>();
                var upstreamKeyEventsAdverseOutcome = aopNetwork.GetUpstreamEffectRelations(adverseOutcome);
                result.UnionWith(aopNetwork.GetUpstreamEffectRelations(commonKeyEvent));
                result.UnionWith(aopNetwork.GetDownstreamEffectRelations(commonKeyEvent));
                result.IntersectWith(upstreamKeyEventsAdverseOutcome);
                return new AdverseOutcomePathwayNetwork() {
                    Code = $"{aopNetwork.Code}_sub_{commonKeyEvent.Code}_{adverseOutcome.Code}",
                    AdverseOutcome = adverseOutcome,
                    EffectRelations = result.ToList()
                };
            } else {
                return new AdverseOutcomePathwayNetwork() {
                    Code = $"{aopNetwork.Code}_sub_{adverseOutcome.Code}",
                    AdverseOutcome = adverseOutcome,
                    EffectRelations = aopNetwork.GetUpstreamEffectRelations(adverseOutcome).ToList()
                };
            }
        }

        /// <summary>
        /// Returns the upstream effect relationships of the specified effect (i.e., the
        /// effects / key events that lead to the specified effect).
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <param name="commonKeyEvent"></param>
        /// <returns></returns>
        public static ICollection<EffectRelationship> GetUpstreamEffectRelations(
            this AdverseOutcomePathwayNetwork aopNetwork,
            Effect commonKeyEvent
        ) {
            var result = new HashSet<EffectRelationship>();
            var upstreamEffects = aopNetwork.EffectRelations
                .Where(r => r.DownstreamKeyEvent == commonKeyEvent)
                .ToList();
            result.UnionWith(upstreamEffects);
            foreach (var effect in upstreamEffects) {
                var childPaths = aopNetwork.GetUpstreamEffectRelations(effect.UpstreamKeyEvent);
                result.UnionWith(childPaths);
            }
            return result;
        }

        /// <summary>
        /// Returns the downstream effect relationships of the specified effect (i.e., the
        /// effects / key events caused by the specified effect).
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <param name="commonKeyEvent"></param>
        /// <returns></returns>
        public static ICollection<EffectRelationship> GetDownstreamEffectRelations(
            this AdverseOutcomePathwayNetwork aopNetwork,
            Effect commonKeyEvent
        ) {
            var result = new HashSet<EffectRelationship>();
            var upstreamEffects = aopNetwork.EffectRelations
                .Where(r => r.UpstreamKeyEvent == commonKeyEvent)
                .ToList();
            result.UnionWith(upstreamEffects);
            foreach (var effect in upstreamEffects) {
                var childPaths = aopNetwork.GetDownstreamEffectRelations(effect.DownstreamKeyEvent);
                result.UnionWith(childPaths);
            }
            return result;
        }

        /// <summary>
        /// Finds key-event relationships that feed-back to earlier key events, and therewith
        /// cause cycles.
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static ICollection<EffectRelationship> FindFeedbackRelationships(this AdverseOutcomePathwayNetwork aopNetwork) {
            var toNodesLookup = aopNetwork.EffectRelations.Select(r => r.DownstreamKeyEvent).ToHashSet();
            var fromNodesLookup = aopNetwork.EffectRelations.ToLookup(r => r.UpstreamKeyEvent);
            var rootNodes = aopNetwork.GetAllEffects().Where(r => !toNodesLookup.Contains(r)).ToList();
            var result = aopNetwork.EffectRelations
                .Where(r => rootNodes.Contains(r.UpstreamKeyEvent))
                .SelectMany(r => findFeedbackRelationshipsRecursive(r, fromNodesLookup, []))
                .Distinct()
                .ToList();
            return result;
        }

        /// <summary>
        /// Identifies key-event relationships that can also be reached indirectly (and may be
        /// non-adjecent).
        /// </summary>
        /// <param name="aopNetwork"></param>
        /// <returns></returns>
        public static ICollection<EffectRelationship> GetIndirectKeyEventRelationships(this AdverseOutcomePathwayNetwork aopNetwork) {
            var cyclicKers = aopNetwork.FindFeedbackRelationships();
            var kers = aopNetwork.EffectRelations.Except(cyclicKers);
            var toNodesLookup = kers.Select(r => r.DownstreamKeyEvent).ToHashSet();
            var fromNodesLookup = kers.ToLookup(r => r.UpstreamKeyEvent);
            var rootNodes = aopNetwork.GetAllEffects().Where(r => !toNodesLookup.Contains(r)).ToList();
            var indirectRelationships = new HashSet<EffectRelationship>();
            foreach (var node in rootNodes) {
                _ = getIndirectRelationshipsRecursive(
                    node,
                    fromNodesLookup,
                    indirectRelationships,
                    [node]
                );
            }
            return indirectRelationships;
        }

        public static void PrintSubNetwork(
            this AdverseOutcomePathwayNetwork aopNetwork,
            Effect commonKeyEvent
        ) {
            var subNet = aopNetwork.GetSubNetwork(aopNetwork.AdverseOutcome, commonKeyEvent);
            var upstreamLookup = subNet.EffectRelations.Select(r => r.DownstreamKeyEvent).ToHashSet();
            var rootEffects = subNet.EffectRelations
                .Select(r => r.UpstreamKeyEvent)
                .Distinct()
                .Where(r => !upstreamLookup.Contains(r))
                .ToList();
            var downstreamLookup = subNet.EffectRelations.ToLookup(r => r.UpstreamKeyEvent);
            var paths = rootEffects.SelectMany(r => getPathsRecursive(r, downstreamLookup));
            foreach (var path in paths) {
                System.Diagnostics.Debug.WriteLine(string.Join(" - ", path.Select(r => r.Code)));
            }
        }

        private static ICollection<EffectRelationship> findFeedbackRelationshipsRecursive(
            EffectRelationship keyEventRelationship,
            ILookup<Effect, EffectRelationship> fromNodesLookup,
            HashSet<EffectRelationship> visited
        ) {
            if (visited.Contains(keyEventRelationship)) {
                return [keyEventRelationship];
            } else {
                visited = visited.ToHashSet();
                visited.Add(keyEventRelationship);
                var linkedKeyEventRelationships = fromNodesLookup.Contains(keyEventRelationship.UpstreamKeyEvent)
                    ? fromNodesLookup[keyEventRelationship.DownstreamKeyEvent].ToList()
                    : [];
                return linkedKeyEventRelationships
                    .SelectMany(r => findFeedbackRelationshipsRecursive(r, fromNodesLookup, visited))
                    .Distinct()
                    .ToList();
            }
        }

        private static List<List<Effect>> getPathsRecursive(
            Effect effect,
            ILookup<Effect, EffectRelationship> downstreamLookup
        ) {
            var result = new List<List<Effect>>();
            if (downstreamLookup.Contains(effect)) {
                var downstreamEffects = downstreamLookup[effect];
                foreach (var downstreamEffect in downstreamEffects) {
                    var downstreamPaths = getPathsRecursive(downstreamEffect.DownstreamKeyEvent, downstreamLookup);
                    result.AddRange(downstreamPaths);
                }
                foreach (var record in result) {
                    record.Insert(0, effect);
                }
            } else {
                result.Add([effect]);
            }
            return result;
        }

        private static HashSet<Effect> getIndirectRelationshipsRecursive(
            Effect keyEvent,
            ILookup<Effect, EffectRelationship> fromNodesLookup,
            HashSet<EffectRelationship> indirectRelationships,
            HashSet<Effect> visited
        ) {
            var result = new HashSet<Effect>();
            var toNodes = fromNodesLookup.Contains(keyEvent)
                ? fromNodesLookup[keyEvent]
                    .Where(r => r.DownstreamKeyEvent != keyEvent)
                    .ToList()
                : [];

            var allIndirectToNodes = new HashSet<Effect>();
            foreach (var toNode in toNodes) {
                if (!visited.Contains(toNode.DownstreamKeyEvent)) {
                    visited.Add(toNode.DownstreamKeyEvent);
                    allIndirectToNodes.UnionWith(
                        getIndirectRelationshipsRecursive(toNode.DownstreamKeyEvent, fromNodesLookup, indirectRelationships, visited)
                    );
                }
            }

            indirectRelationships.UnionWith(toNodes.Where(r => allIndirectToNodes.Contains(r.DownstreamKeyEvent)));
            allIndirectToNodes.UnionWith(toNodes.Select(r => r.DownstreamKeyEvent));
            return allIndirectToNodes;
        }
    }
}
