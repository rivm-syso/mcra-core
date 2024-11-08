using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    ///  Class for generating mock assessment group memberships
    /// </summary>
    public static class FakeAssessmentGroupMembershipModelsGenerator {
        /// <summary>
        /// Creates  assessment group membership models based on scores
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="scores"></param>
        /// <param name="accurary"></param>
        /// <param name="sensitivity"></param>
        /// <param name="specificity"></param>
        /// <returns></returns>
        public static ActiveSubstanceModel Create(
            Effect effect,
            ICollection<Compound> substances,
            double[] scores,
            double accurary = 0.8,
            double sensitivity = 0.8,
            double specificity = 0.8
        ) {
            var result = new ActiveSubstanceModel() {
                Effect = effect,
                Code = $"AG-{effect.Code}",
                Name = $"AG-{effect.Code}",
                Description = $"AG-{effect.Code}",
                Accuracy = accurary,
                Sensitivity = sensitivity,
                Specificity = specificity,
                MembershipProbabilities = new Dictionary<Compound, double>()
            };
            for (int i = 0; i < substances.Count; i++) {
                if (!double.IsNaN(scores[i])) {
                    result.MembershipProbabilities[substances.ElementAt(i)] = scores[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Creates  assessment group membership models based on random scores
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="pPos"></param>
        /// <param name="accurary"></param>
        /// <param name="sensitivity"></param>
        /// <param name="specificity"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static ActiveSubstanceModel Create(
            Effect effect,
            ICollection<Compound> substances,
            double pPos = 0.5,
            double accurary = 0.8,
            double sensitivity = 0.8,
            double specificity = 0.8,
            int seed = 1
        ) {
            var rnd = new McraRandomGenerator(seed);
            return new ActiveSubstanceModel() {
                Effect = effect,
                Code = $"AG-{effect.Code}",
                Name = $"AG-{effect.Code}",
                Description = $"AG-{effect.Code}",
                Accuracy = accurary,
                Sensitivity = sensitivity,
                Specificity = specificity,
                MembershipProbabilities = substances.ToDictionary(r => r, r => rnd.NextDouble() > pPos ? 0D : 1D)
            };
        }
    }
}
