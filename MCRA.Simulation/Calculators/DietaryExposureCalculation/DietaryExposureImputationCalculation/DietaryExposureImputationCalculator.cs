using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.DietaryExposureImputationCalculation {
    public sealed class DietaryExposureImputationCalculator {

        /// <summary>
        /// Imputation method for nominal run.
        /// </summary>
        /// <param name="exposureType"></param>
        /// <param name="exposurePerCompound"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<DietaryIndividualDayIntake> Impute(
            ExposureType exposureType,
            Dictionary<Compound, List<ExposureRecord>> exposurePerCompound,
            IDictionary<(Food, Compound), CompoundResidueCollection> compoundResidueCollections,
            ICollection<Compound> activeSubstances,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IRandom random
        ) {
            var compoundsWithExposure = compoundResidueCollections.Values
                .Where(r => r.NumberOfResidues > 0)
                .Select(r => r.Compound)
                .ToHashSet();

            var missingCompounds = activeSubstances
                .Where(r => !compoundsWithExposure.Contains(r))
                .ToHashSet();

            // Compute means (log scale) and fractions of positive exposures
            var results = exposurePerCompound
                 .Where(c => compoundsWithExposure.Contains(c.Key))
                 .Select(c => {
                     var result = c.Value.Where(i => i.Exposure > 0).ToList();
                     return (
                         Compound: c.Key,
                         Mu: result.Sum(a => Math.Log(a.Exposure) * a.SamplingWeight) / result.Sum(a => a.SamplingWeight),
                         FractionPositives: result.Sum(a => a.SamplingWeight) / dietaryIndividualDayIntakes.Sum(a => a.SimulatedIndividual.SamplingWeight)
                     );
                 }).ToList();

            // Weighted mean of combined distribution (for all compounds)
            var weightedMean = results
                .Where(r => r.FractionPositives > 0)
                .Sum(c => c.Mu * c.FractionPositives) / results.Sum(c => c.FractionPositives);

            // Combine exposures of all compounds with exposure in a combined distribution
            // all distributions are shifted towards the weighted mean
            var defaultDistribution = new List<double>();
            foreach (var record in exposurePerCompound) {
                var result = results.SingleOrDefault(c => c.Compound == record.Key);
                if (result != default) {
                    var shift = !double.IsNaN(result.Mu) ? Math.Exp(weightedMean - result.Mu) : 1D;
                    defaultDistribution.AddRange(record.Value.Select(c => c.Exposure * shift).ToList());
                }
            }
            foreach (var compound in missingCompounds) {
                imputeExposures(exposureType, dietaryIndividualDayIntakes, defaultDistribution, compound, random);
            }
            return dietaryIndividualDayIntakes.ToList();
        }

        /// <summary>
        /// Imputation method for uncertainty runs.
        /// </summary>
        /// <param name="exposureType"></param>
        /// <param name="ExposurePerCompoundRecords"></param>
        /// <param name="compoundResidueCollections"></param>
        /// <param name="activeSubstances"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="variabilityRandomGenerator"></param>
        /// <param name="uncertaintyRandomGenerator"></param>
        /// <returns></returns>
        public List<DietaryIndividualDayIntake> ImputeUncertaintyRun(
            ExposureType exposureType,
            Dictionary<Compound, List<ExposureRecord>> ExposurePerCompoundRecords,
            IDictionary<(Food, Compound), CompoundResidueCollection> compoundResidueCollections,
            ICollection<Compound> activeSubstances,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IRandom variabilityRandomGenerator,
            IRandom uncertaintyRandomGenerator
        ) {
            var compoundsWithExposure = compoundResidueCollections
                .Where(r => r.Value.NumberOfResidues > 0)
                .Select(r => r.Value.Compound)
                .ToHashSet();
            var missingCompounds = activeSubstances
                .Where(r => !compoundsWithExposure.Contains(r))
                .ToHashSet();
            var positiveIntakeCompoundRecords = ExposurePerCompoundRecords
                .Where(c => compoundsWithExposure.Contains(c.Key))
                .ToArray();
            foreach (var compound in missingCompounds) {
                // Uncertainty: draw exposures of random other substance
                var drawCompound = positiveIntakeCompoundRecords[uncertaintyRandomGenerator.Next(0, positiveIntakeCompoundRecords.Length)];
                // Variability: impute exposures
                imputeExposures(
                    exposureType,
                    dietaryIndividualDayIntakes,
                    drawCompound.Value.Select(c => c.Exposure).ToList(),
                    compound,
                    variabilityRandomGenerator
                );
            }
            return dietaryIndividualDayIntakes.ToList();
        }

        private void imputeExposures(
            ExposureType exposureType,
            IEnumerable<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            List<double> exposures,
            Compound compound,
            IRandom random
        ) {
            if (exposureType == ExposureType.Acute) {
                foreach (var day in dietaryIndividualDayIntakes) {
                    var ix = random.Next(0, exposures.Count);
                    day.OtherIntakesPerCompound.Add(new AggregateIntakePerCompound() {
                        Compound = compound,
                        Amount = exposures[ix],
                    });
                }
            } else {
                var individualIds = dietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.Id).Distinct().ToList();
                foreach (var individualId in individualIds) {
                    var ix = random.Next(0, exposures.Count);
                    var individualDays = dietaryIndividualDayIntakes.Where(c => c.SimulatedIndividual.Id == individualId).ToList();
                    foreach (var day in individualDays) {
                        day.OtherIntakesPerCompound.Add(new AggregateIntakePerCompound() {
                            Compound = compound,
                            Amount = exposures[ix],
                        });
                    }
                }
            }
        }
    }
}
