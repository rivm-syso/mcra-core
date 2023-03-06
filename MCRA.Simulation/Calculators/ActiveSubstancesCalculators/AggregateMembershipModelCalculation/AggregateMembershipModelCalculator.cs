using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation {

    /// <summary>
    /// Calculator class for computing aggregate assessment group memberships
    /// from multiple assessment group membership models.
    /// </summary>
    public class AggregateMembershipModelCalculator {

        private readonly bool _isProbabilistic;
        private readonly bool _includeSubstancesWithUnknowMemberships;
        private readonly bool _bubbleMembershipsThroughAop;
        private readonly double _priorMembershipProbability;
        private readonly AssessmentGroupMembershipCalculationMethod _assessmentGroupMembershipCalculationMethod;
        private readonly CombinationMethodMembershipInfoAndPodPresence _combinationMethodMembershipInfoAndPodPresence;

        /// <summary>
        /// Creates a new <see cref="AggregateMembershipModelCalculator"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        public AggregateMembershipModelCalculator(
            IAggregateMembershipModelCalculatorSettings settings
        ) {
            _includeSubstancesWithUnknowMemberships = settings.IncludeSubstancesWithUnknowMemberships;
            _bubbleMembershipsThroughAop = settings.BubbleMembershipsThroughAop;
            _assessmentGroupMembershipCalculationMethod = settings.AssessmentGroupMembershipCalculationMethod;
            _priorMembershipProbability = settings.PriorMembershipProbability;
            _isProbabilistic = settings.UseProbabilisticMemberships;
            _combinationMethodMembershipInfoAndPodPresence = settings.CombinationMethodMembershipInfoAndPodPresence;
        }

        /// <summary>
        /// Computes the aggregate membership model.
        /// </summary>
        /// <param name="models"></param>
        /// <param name="substances"></param>
        /// <param name="effect"></param>
        /// <param name="upstreamEffectsLookup"></param>
        /// <returns></returns>
        public ActiveSubstanceModel Compute(
            ICollection<ActiveSubstanceModel> models,
            ICollection<Compound> substances,
            Effect effect,
            ILookup<Effect, EffectRelationship> upstreamEffectsLookup
        ) {
            ActiveSubstanceModel result;
            if (_bubbleMembershipsThroughAop && upstreamEffectsLookup != null) {
                var modelsByEffect = ComputeAllUpstreamEffectMembershipModels(models, substances, effect, upstreamEffectsLookup);
                if (!modelsByEffect.TryGetValue(effect, out result)) {
                    result = compute(models, substances, _includeSubstancesWithUnknowMemberships, false, effect);
                }
            } else {
                result = compute(models, substances, _includeSubstancesWithUnknowMemberships, false, effect);
            }
            return result;
        }

        public Dictionary<Effect, ActiveSubstanceModel> ComputeAllUpstreamEffectMembershipModels(
            ICollection<ActiveSubstanceModel> models,
            ICollection<Compound> compounds,
            Effect effect,
            ILookup<Effect, EffectRelationship> upstreamEffectsLookup
        ) {
            var modelsByEffect = new Dictionary<Effect, ActiveSubstanceModel>();
            computeMembershipsRecursive(models.ToLookup(r => r.Effect), compounds, effect, modelsByEffect, upstreamEffectsLookup);
            return modelsByEffect;
        }

        public Dictionary<Compound, double> MergeMembershipsWithPodAvailability(
            ICollection<Compound> substances,
            ActiveSubstanceModel membershipsFromPodPresenceModel,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            switch (_combinationMethodMembershipInfoAndPodPresence) {
                case CombinationMethodMembershipInfoAndPodPresence.Intersection:
                    return substances.ToDictionary(r => r, r => membershipsFromPodPresenceModel.MembershipProbabilities[r] >= .5 ? membershipProbabilities[r] : 0D);
                case CombinationMethodMembershipInfoAndPodPresence.Union:
                    return substances.ToDictionary(r => r, r => membershipsFromPodPresenceModel.MembershipProbabilities[r] >= 0.5 ? 1D : membershipProbabilities[r]);
                default:
                    throw new NotImplementedException();
            }
        }

        private void computeMembershipsRecursive(
            ILookup<Effect, ActiveSubstanceModel> availableMembershipModels,
            ICollection<Compound> compounds,
            Effect effect,
            Dictionary<Effect, ActiveSubstanceModel> computedModels,
            ILookup<Effect, EffectRelationship> upstreamEffectsLookup
        ) {
            if (!computedModels.ContainsKey(effect)) {
                var candidateMembershipModels = new List<ActiveSubstanceModel>();
                if (upstreamEffectsLookup.Contains(effect)) {
                    var upstreamEffects = upstreamEffectsLookup[effect].ToList();
                    foreach (var upstreamEffect in upstreamEffects) {
                        if (!computedModels.TryGetValue(upstreamEffect.UpstreamKeyEvent, out var upstreamEffectMemberships)) {
                            computeMembershipsRecursive(availableMembershipModels, compounds, upstreamEffect.UpstreamKeyEvent, computedModels, upstreamEffectsLookup);
                            upstreamEffectMemberships = computedModels[upstreamEffect.UpstreamKeyEvent];
                        }
                        if (upstreamEffectMemberships != null) {
                            candidateMembershipModels.Add(upstreamEffectMemberships);
                        }
                    }
                }
                if (availableMembershipModels.Contains(effect)) {
                    candidateMembershipModels.AddRange(availableMembershipModels[effect]);
                }
                computedModels[effect] = candidateMembershipModels.Any()
                    ? compute(candidateMembershipModels, compounds, false, true, effect) : null;
            }
        }

        private ActiveSubstanceModel compute(
            ICollection<ActiveSubstanceModel> availableMembershipModels,
            ICollection<Compound> substances,
            bool includeSubstancesWithUnknowMemberships,
            bool missingIsNaN,
            Effect effect
        ) {
            if (_assessmentGroupMembershipCalculationMethod == AssessmentGroupMembershipCalculationMethod.ProbabilisticBayesian) {
                if (availableMembershipModels.Any(r => !r.Specificity.HasValue || double.IsNaN(r.Specificity.Value) || !r.Sensitivity.HasValue || double.IsNaN(r.Sensitivity.Value))) {
                    throw new Exception("Error Bayesian approach of computing assessment group membership probabilities requires sensitivity and specificity values for all selected QSAR membership models and docking models.");
                }
            }

            var computedMemberships = substances.ToDictionary(c => c, c => double.NaN);
            foreach (var substance in substances) {

                // Collect compound memberships scores for all models (skip if not available)
                var availableMemberships = availableMembershipModels
                    .Where(r => r.MembershipProbabilities.TryGetValue(substance, out var score) && !double.IsNaN(score))
                    .ToList();

                // Collect compound memberships scores for all models (skip if not available)
                var membershipScores = availableMemberships
                    .Select(r => r.MembershipProbabilities[substance])
                    .ToList();

                if (membershipScores.Any()) {
                    switch (_assessmentGroupMembershipCalculationMethod) {
                        case AssessmentGroupMembershipCalculationMethod.CrispMax:
                            computedMemberships[substance] = membershipScores.Max();
                            break;
                        case AssessmentGroupMembershipCalculationMethod.CrispMajority:
                            computedMemberships[substance] = membershipScores.Sum() >= .5 * membershipScores.Count ? 1D : 0D;
                            break;
                        case AssessmentGroupMembershipCalculationMethod.ProbabilisticRatio:
                            computedMemberships[substance] = (double)membershipScores.Sum() / membershipScores.Count;
                            break;
                        case AssessmentGroupMembershipCalculationMethod.ProbabilisticBayesian:
                            computedMemberships[substance] = (double)membershipScores.Aggregate((total, next) => total * next);

                            var pGiven1 = availableMemberships
                                .Select(r => r.MembershipProbabilities[substance] < .5 ? 1 - r.Sensitivity.Value : r.Sensitivity.Value)
                                .Aggregate((total, next) => total * next);
                            var pGiven0 = availableMemberships
                                .Select(r => r.MembershipProbabilities[substance] < .5 ? r.Specificity.Value : 1 - r.Specificity.Value)
                                .Aggregate((total, next) => total * next);
                            var pq = _priorMembershipProbability * pGiven1 + (1 - _priorMembershipProbability) * pGiven0;
                            var probability = _priorMembershipProbability * pGiven1 / pq;
                            computedMemberships[substance] = probability;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                } else {
                    if (missingIsNaN) {
                        computedMemberships[substance] = double.NaN;
                    } else {
                        if (_isProbabilistic) {
                            computedMemberships[substance] = _priorMembershipProbability;
                        } else if (includeSubstancesWithUnknowMemberships) {
                            computedMemberships[substance] = 1D;
                        } else {
                            computedMemberships[substance] = 0D;
                        }
                    }
                }
            }

            return new ActiveSubstanceModel() {
                Code = effect != null ? $"AG-{effect.Code}" : "AG",
                Name = effect != null ? $"AG {effect.Name}" : "AG",
                Description = $"Computed from {string.Join(", ", availableMembershipModels.Select(r => r.Name))}",
                Effect = effect,
                MembershipProbabilities = computedMemberships,
            };
        }
    }
}
