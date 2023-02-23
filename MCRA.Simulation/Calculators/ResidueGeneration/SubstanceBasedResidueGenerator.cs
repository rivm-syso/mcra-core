using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public sealed class SubstanceBasedResidueGenerator : IResidueGenerator {

        private IResidueGeneratorSettings _settings;
        /// <summary>
        /// The concentration models from which the residues are drawn.
        /// </summary>
        private readonly IDictionary<(Food, Compound), ConcentrationModel> _concentrationModels;

        /// <summary>
        /// If occurrence patterns should be used, these are the patterns to draw from.
        /// </summary>
        private readonly IDictionary<Food, List<MarginalOccurrencePattern>> _marginalOccurrencePatterns;

        public SubstanceBasedResidueGenerator(
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IDictionary<Food, List<MarginalOccurrencePattern>> marginalOccurrencePatterns,
            IResidueGeneratorSettings settings
        ) {
            _concentrationModels = concentrationModels;
            _marginalOccurrencePatterns = marginalOccurrencePatterns;
            _settings = settings;
        }

        /// <summary>
        /// Draws residues for the target food and substances using the supplied random generator and using a multicompound algorithm for drawing residues.
        /// </summary>
        /// <param name="foodAsMeasured"></param>
        /// <param name="compounds"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<CompoundConcentration> GenerateResidues(Food foodAsMeasured, ICollection<Compound> compounds, IRandom random) {
            if (_settings.UseOccurrencePatternsForResidueGeneration) {
                // 3) not samplebased, use agricultural use percentages
                return generateForOccurrencePattern(foodAsMeasured, compounds, random);
            } else {
                // 4) not samplebased, don't use agricultural use percentages
                return generate(foodAsMeasured, compounds, random);
            }
        }

        /// <summary>
        /// Multiple compounds, NOT samplebased 
        /// 1) Do not use authorized use data: then set agricultural use to 100% for all food / compound combinations
        /// 2) Use agricultural use: agricultural use = 100% for allowed food / compound combinations
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private List<CompoundConcentration> generate(
            Food food,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var concentrations = new List<CompoundConcentration>();
            foreach (var compound in substances) {
                if (_concentrationModels[(food, compound)].CorrectedWeightedAgriculturalUseFraction > 0) {
                    var model = _concentrationModels[(food, compound)];
                    var concentration = model.DrawFromDistribution(random, _settings.NonDetectsHandlingMethod);
                    if (concentration > 0) {
                        concentrations.Add(new CompoundConcentration() {
                            Compound = compound,
                            Concentration = (float)concentration,
                        });
                    }
                }
            }
            return concentrations;
        }

        /// <summary>
        /// 1) Not samplebased, based on agricultural use percentages
        /// </summary>
        /// <param name="foodAsMeasured"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private List<CompoundConcentration> generateForOccurrencePattern(
            Food foodAsMeasured,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var authorized = true;
            MarginalOccurrencePattern drawnOccurrencePattern = null;
            if (_marginalOccurrencePatterns.TryGetValue(foodAsMeasured, out var foodOccurrencePatterns)) {
                var agriculturalUses = foodOccurrencePatterns;
                var numberOfSpecifiedAgriculturalUseGroups = agriculturalUses.Count;
                drawnOccurrencePattern = agriculturalUses.DrawRandom(random, au => au.OccurrenceFraction);
                if (drawnOccurrencePattern == null && _settings.TreatMissingOccurrencePatternsAsNotOccurring) {
                    authorized = false;
                }
            } else {
                // If no agricultural uses are specified at all, then we should assume that use is authorized for all compounds
                authorized = true;
            }

            var concentrations = new List<CompoundConcentration>();
            foreach (var substance in substances) {
                var isSubstanceInPattern = false;
                var concentration = 0D;
                var model = _concentrationModels[(foodAsMeasured, substance)];
                if (drawnOccurrencePattern != null) {
                    isSubstanceInPattern = drawnOccurrencePattern.Compounds.Contains(substance);
                    if (isSubstanceInPattern) {
                        concentration = model.DrawFromDistributionExceptZeroes(random, _settings.NonDetectsHandlingMethod);
                    }
                } else {
                    if (authorized) {
                        concentration = model.DrawFromDistributionExceptZeroes(random, _settings.NonDetectsHandlingMethod);
                    }
                }
                if (model.FractionPositives > model.WeightedAgriculturalUseFraction) {
                    var p = (model.FractionPositives - model.WeightedAgriculturalUseFraction) / (1 - model.WeightedAgriculturalUseFraction);
                    if (random.NextDouble() < p) {
                        concentration = model.DrawFromDistributionExceptZeroes(random, _settings.NonDetectsHandlingMethod);
                    }
                }
                if (concentration > 0) {
                    concentrations.Add(new CompoundConcentration() {
                        Compound = substance,
                        Concentration = (float)concentration,
                    });
                }
            }
            return concentrations;
        }
    }
}
