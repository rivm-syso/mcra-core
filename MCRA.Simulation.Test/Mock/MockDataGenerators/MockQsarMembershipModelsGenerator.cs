using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock QSAR membership models
    /// </summary>
    public static class MockQsarMembershipModelsGenerator {
        /// <summary>
        /// Creates a QSAR membership model
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="scores"></param>
        /// <param name="accuracy"></param>
        /// <param name="sensitivity"></param>
        /// <param name="specificity"></param>
        /// <returns></returns>
        public static QsarMembershipModel Create(
            Effect effect,
            ICollection<Compound> substances,
            double[] scores,
            double accuracy = 0.8,
            double sensitivity = 0.8,
            double specificity = 0.8
        ) {
            var result = new QsarMembershipModel() {
                Effect = effect,
                Code = $"QSAR-{effect.Code}",
                Name = $"QSAR-{effect.Code}",
                Description = $"QSAR-{effect.Code}",
                Accuracy = accuracy,
                Sensitivity = sensitivity,
                Specificity = specificity,
                MembershipScores = new Dictionary<Compound, double>()
            };
            for (int i = 0; i < substances.Count; i++) {
                result.MembershipScores[substances.ElementAt(i)] = scores[i];
            }
            return result;
        }
        /// <summary>
        /// Creates a QSAR membership model
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="pPos"></param>
        /// <param name="accurary"></param>
        /// <param name="sensitivity"></param>
        /// <param name="specificity"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static QsarMembershipModel Create(
            Effect effect,
            ICollection<Compound> substances,
            double pPos = 0.5,
            double accurary = 0.8,
            double sensitivity = 0.8,
            double specificity = 0.8,
            int seed = 1
        ) {
            var rnd = new McraRandomGenerator(seed);
            return new QsarMembershipModel() {
                Effect = effect,
                Code = $"QSAR-{effect.Code}",
                Name = $"QSAR-{effect.Code}",
                Description = $"QSAR-{effect.Code}",
                Accuracy = accurary,
                Sensitivity = sensitivity,
                Specificity = specificity,
                MembershipScores = substances.ToDictionary(r => r, r => rnd.NextDouble() > pPos ? 0D : 1D)
            };
        }
    }
}
