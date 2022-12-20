using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.WaterConcentrationsExtrapolation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class WaterConcentrationsExtrapolationCalculator {

        private readonly IWaterConcentrationsExtrapolationCalculatorSettings _settings;

        public WaterConcentrationsExtrapolationCalculator(IWaterConcentrationsExtrapolationCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Water imputation: adds additional samples for water.
        /// </summary>
        /// <param name="activeSubstances"></param>
        /// <param name="food"></param>
        /// <param name="imputationValue"></param>
        /// <param name="authorisations"></param>
        /// <param name="onlyImputeMostPotentSubstances"></param>
        /// <param name="numberOfSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public SampleCompoundCollection Create(
            ICollection<Compound> activeSubstances,
            Food food,
            IDictionary<(Food, Compound), SubstanceAuthorisation> authorisations,
            int numberOfSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ConcentrationUnit concentrationUnit
        ) {
            authorisations = _settings.RestrictWaterImputationToAuthorisedUses ? authorisations : null;

            var imputationSubstances = (_settings.RestrictWaterImputationToMostPotentSubstances && activeSubstances.Count > numberOfSubstances)
                ? getMostPotentSubstances(activeSubstances, food, authorisations, relativePotencyFactors, numberOfSubstances)
                : activeSubstances;

            var concentrationUnitCorrectionFactor = ConcentrationUnit.ugPerKg.GetConcentrationUnitMultiplier(concentrationUnit);
            var residueValue = concentrationUnitCorrectionFactor * _settings.WaterConcentrationValue;
            var sampleCompoundRecord = new SampleCompoundRecord() {
                AuthorisedUse = true,
                SampleCompounds = activeSubstances
                    .ToDictionary(r => r, r => {
                        return new SampleCompound() {
                            ActiveSubstance = r,
                            MeasuredSubstance = r,
                            IsExtrapolated = true,
                            Residue = imputationSubstances.Contains(r) ? residueValue : 0D
                        };
                    })
            };
            var sampleCompoundRecords = new List<SampleCompoundRecord>() { sampleCompoundRecord };

            var result = new SampleCompoundCollection(food, sampleCompoundRecords);
            return result;
        }

        private static HashSet<Compound> getMostPotentSubstances(
            ICollection<Compound> activeSubstances,
            Food food,
            IDictionary<(Food, Compound), SubstanceAuthorisation> authorisations,
            IDictionary<Compound, double> relativePotencyFactors,
            int takeNumber
        ) {
            if (relativePotencyFactors == null) {
                throw new Exception($"Missing RPFs for imputation of water concentrations for {takeNumber} most potent substances.");
            }
            return activeSubstances
                    .Where(r => authorisations?.ContainsKey((food, r)) ?? true)
                    .OrderByDescending(r => relativePotencyFactors.ContainsKey(r) ? relativePotencyFactors[r] : 0D)
                    .Take(takeNumber)
                    .ToHashSet();
        }
    }
}
