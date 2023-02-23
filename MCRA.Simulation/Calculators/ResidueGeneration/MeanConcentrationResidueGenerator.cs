using MCRA.Utils.Collections;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public sealed class MeanConcentrationResidueGenerator : IResidueGenerator {

        /// <summary>
        /// The concentration models from which the residues are drawn.
        /// </summary>
        private IDictionary<(Food Food, Compound Substance), ConcentrationModel> _concentrationModels;

        /// <summary>
        /// Dictionary in which the residue means are stored in order to access them more quickly.
        /// </summary>
        private IDictionary<(Food, Compound), CompoundConcentration> _residueMeansDictionary;

        public MeanConcentrationResidueGenerator(IDictionary<(Food, Compound), ConcentrationModel> concentrationModels) {
            _concentrationModels = concentrationModels;
        }

        /// <summary>
        /// Generate mean residues for all food substance combinations.
        /// </summary>
        /// <returns></returns>
        public void Initialize() {
            _residueMeansDictionary = new Dictionary<(Food, Compound), CompoundConcentration>();
            foreach (var model in _concentrationModels) {
                var concentration = model.Value.GetDistributionMean();
                _residueMeansDictionary.Add(model.Key, new CompoundConcentration() {
                    Compound = model.Key.Substance,
                    Concentration = (float)concentration,
                });
            }
        }

        /// <summary>
        /// Returns the mean concentrations of the target substances on the target food with a correction for agricultural use percentages.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public List<CompoundConcentration> GetResidueMeans(Food foodAsMeasured, ICollection<Compound> substances) {
            var concentrations = new List<CompoundConcentration>();
            foreach (var substance in substances) {
                var concentration = _residueMeansDictionary[(foodAsMeasured, substance)];
                if (concentration.Concentration > 0) {
                    concentrations.Add(concentration);
                }
            }
            return concentrations;
        }

        /// <summary>
        /// Implements <see cref="IResidueGenerator.GenerateResidues(Food, ICollection{Compound}, IRandom)" />.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<CompoundConcentration> GenerateResidues(Food food, ICollection<Compound> substances, IRandom random) {
            return GetResidueMeans(food, substances);
        }
    }
}
