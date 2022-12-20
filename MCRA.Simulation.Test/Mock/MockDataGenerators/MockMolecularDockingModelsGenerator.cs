using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock molecular docking models
    /// </summary>
    public static class MockMolecularDockingModelsGenerator {
        /// <summary>
        /// Creates a molecular docking model
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="substances"></param>
        /// <param name="threshold"></param>
        /// <param name="dockingScores"></param>
        /// <returns></returns>
        public static MolecularDockingModel Create(
            Effect effect,
            ICollection<Compound> substances,
            double threshold,
            double[] dockingScores
        ) {
            var result = new MolecularDockingModel() {
                Effect = effect,
                Code = $"DockingModel-{effect.Code}",
                Name = $"DockingModel-{effect.Code}",
                Description = $"DockingModel-{effect.Code}",
                Threshold = threshold,
                BindingEnergies = new Dictionary<Compound, double>()
            };
            for (int i = 0; i < substances.Count; i++) {
                result.BindingEnergies[substances.ElementAt(i)] = dockingScores[i];
            }
            return result;
        }
    }
}
