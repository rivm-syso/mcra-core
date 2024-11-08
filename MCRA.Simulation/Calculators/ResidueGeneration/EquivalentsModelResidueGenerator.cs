using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Calculators.ResidueGeneration {

    public sealed class EquivalentsModelResidueGenerator : IResidueGenerator {

        /// <summary>
        /// The RPF values of the substances.
        /// </summary>
        private readonly IDictionary<Compound, double> _relativePotencyFactors;

        /// <summary>
        /// Cumulative concentration model dictionary.
        /// </summary>
        private readonly IDictionary<Food, ConcentrationModel> _cumulativeConcentrationModels;

        /// <summary>
        /// The sample compound collections used for sample based residue generation
        /// </summary>
        private readonly IDictionary<Food, SampleCompoundCollection> _sampleCompoundCollections;


        /// <summary>
        /// Sample based calculated weights per food and the total + per substance id, the average and RPF.
        /// </summary>
        private IDictionary<Food, SampleBasedWeightsInfo> _sampleBasedWeightsPerFoodAndSubstance;

        /// <summary>
        /// Caluclates cumulative weight of the specified substance for the specified food
        /// (used for sample-based)
        /// </summary>
        /// <returns></returns>
        public void Initialize(ICollection<Compound> substances, ICollection<Food> foods) {
            _sampleBasedWeightsPerFoodAndSubstance = new Dictionary<Food, SampleBasedWeightsInfo>();
            foreach (var modelledFood in foods) {

                //calculate the list once per modelled food
                var sampleCompoundCollection = _sampleCompoundCollections[modelledFood];
                //temporary dictionary to extract the values for all compounds in this food
                var compoundValues = new Dictionary<Compound, List<double>>();
                //loop through all the substances in the SampleCompounds in the SampleCompoundRecords
                foreach (var scr in sampleCompoundCollection.SampleCompoundRecords) {
                    foreach (var cmp in scr.SampleCompounds) {
                        //add the substance if not yet added
                        if (!compoundValues.ContainsKey(cmp.Key)) {
                            compoundValues[cmp.Key] = [];
                        }
                        //add the residue value to the list for the substance
                        compoundValues[cmp.Key].Add(cmp.Value.Residue);
                    }
                }
                var weightsInfo = new SampleBasedWeightsInfo();
                foreach (var cpv in substances) {
                    //get the average and weighted average
                    var average = compoundValues.TryGetValue(cpv, out var residues) ? residues.AverageOrZero() : 0D;
                    //weighted means sum, is the average * the CorrectedRPF factor
                    weightsInfo.SumOfWeightedMeans += average * (_relativePotencyFactors?[cpv] ?? 1D);
                    //save the average per substance, uncorrected for the RPF
                    weightsInfo.AveragesPerCompound.Add(cpv, average);
                }
                _sampleBasedWeightsPerFoodAndSubstance.Add(modelledFood, weightsInfo);
            }
        }


        /// <summary>
        /// No NonDetectsHandlingMethod required for equivalent model
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="cumulativeConcentrationModels"></param>
        /// <param name="sampleCompoundCollections"></param>
        public EquivalentsModelResidueGenerator(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels,
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections
        ) {
            _relativePotencyFactors = relativePotencyFactors;
            _cumulativeConcentrationModels = cumulativeConcentrationModels;
            _sampleCompoundCollections = sampleCompoundCollections;
        }

        #region Classes

        private class SampleBasedWeightsInfo {
            public double SumOfWeightedMeans { get; set; }
            public Dictionary<Compound, double> AveragesPerCompound { get; private set; }
            public SampleBasedWeightsInfo() {
                AveragesPerCompound = [];
            }
        }

        #endregion

        /// <summary>
        /// Draws residues for the target food and substances from the equivalents model using the
        /// supplied random generator.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<CompoundConcentration> GenerateResidues(
            Food food,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var model = _cumulativeConcentrationModels[food];
            var cumulativePotencyResidue = model.DrawFromDistribution(random, NonDetectsHandlingMethod.ReplaceByZero);
            var concentrations = new List<CompoundConcentration>(substances.Count);
            foreach (var compound in substances) {
                concentrations.Add(new CompoundConcentration() {
                    Compound = compound,
                    Concentration = (float)(cumulativePotencyResidue * getSampleBasedWeight(food, compound)),
                });
            }
            return concentrations;
        }

        /// <summary>
        /// Returns the cumulative weight of the specified substance for the specified food
        /// (used for sample-based).
        /// </summary>
        /// <param name="modelledFood"></param>
        /// <param name="substance"></param>
        ///
        /// <returns></returns>
        private double getSampleBasedWeight(
            Food modelledFood,
            Compound substance
        ) {
            var sbw = _sampleBasedWeightsPerFoodAndSubstance[modelledFood];
            var compoundAverage = sbw.AveragesPerCompound.ContainsKey(substance)
                                ? sbw.AveragesPerCompound[substance]
                                : 1D;

            return sbw.SumOfWeightedMeans == 0 ? 0 : compoundAverage / sbw.SumOfWeightedMeans;
        }
    }
}
