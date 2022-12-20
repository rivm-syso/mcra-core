using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.AgriculturalUseInfo;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class OccurrenceFractionsCalculator {

        private readonly IOccurrenceFractionsCalculatorSettings _settings;

        public OccurrenceFractionsCalculator(IOccurrenceFractionsCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Computes agricultural use info for specified foods-as-measured and substances
        /// based on the provided agricultural use data and samples.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="occurrencePatterns"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), OccurrenceFraction> Compute(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            ICollection<MarginalOccurrencePattern> occurrencePatterns
        ) {
            return Compute(
                foods,
                substances,
                occurrencePatterns
                    .GroupBy(r => r.Food)
                    .ToDictionary(r => r.Key, r => r.ToList())
                );
        }

        /// <summary>
        /// Computes agricultural use info for specified modelled foods and substances
        /// based on the provided agricultural use data and samples.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="occurrencePatterns"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), OccurrenceFraction> Compute(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            Dictionary<Food, List<MarginalOccurrencePattern>> occurrencePatterns
        ) {
            var result = new Dictionary<(Food, Compound), OccurrenceFraction>();
            foreach (var food in foods.Where(f => occurrencePatterns.ContainsKey(f))) {
                var foodAgriculturalUses = occurrencePatterns[food];
                foreach (var substance in substances) {
                    var relevantUses = foodAgriculturalUses
                        .Where(r => r.Compounds.Contains(substance))
                        .ToList();
                    var fraction = relevantUses.Sum(r => r.OccurrenceFraction);
                    if (!_settings.UseAgriculturalUsePercentage) {
                        fraction = (fraction > 0) ? 1 : 0;
                    }
                    var locationUseRecord = new LocationOccurrenceFraction() {
                        Location = string.Empty,
                        FractionAllSamples = 1D,
                        SubstanceUseFound = fraction > 0,
                        OccurrenceFraction = fraction
                    };
                    var record = new OccurrenceFraction() {
                        Food = food,
                        Substance = substance,
                        OccurrenceFrequency = fraction,
                        LocationOccurrenceFractions = new Dictionary<string, ILocationOccurrenceFrequency>() {
                            { string.Empty, locationUseRecord }
                        }
                    };
                    result.Add((food, substance), record);
                }
            }
            return result;
        }

        /// <summary>
        /// Computes location-based occurrence fractions for specified modelled foods and
        /// substances based on the provided occurrence patterns and samples.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="occurrencePatterns"></param>
        /// <param name="sampleOrigins"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), OccurrenceFraction> ComputeLocationBased(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            ICollection<OccurrencePattern> occurrencePatterns,
            IDictionary<Food, List<ISampleOrigin>> sampleOrigins
        ) {
            var result = new Dictionary<(Food, Compound), OccurrenceFraction>();
            var agriculturalUsesPerFood = occurrencePatterns.ToLookup(au => au.Food);
            foreach (var f in foods.Where(f => agriculturalUsesPerFood.Contains(f))) {
                var foodAgriculturalUses = agriculturalUsesPerFood[f].ToList();
                var foodSampleOrigins = sampleOrigins.ContainsKey(f) ? sampleOrigins[f] : new List<ISampleOrigin>();
                var compoundAgriculturalUses = computeLocationOccurrenceFrequencies(f, substances, foodAgriculturalUses, foodSampleOrigins);
                foreach (var record in compoundAgriculturalUses) {
                    result.Add((f, record.Substance), record);
                }
            }
            return result;
        }

        /// <summary>
        /// Calculates the agricultural use info per substance.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="compounds"></param>
        /// <param name="agriculturalUses"></param>
        /// <param name="sampleOrigins"></param>
        /// <returns></returns>
        private List<OccurrenceFraction> computeLocationOccurrenceFrequencies(
            Food food,
            ICollection<Compound> compounds,
            List<OccurrencePattern> agriculturalUses,
            ICollection<ISampleOrigin> sampleOrigins
        ) {
            var records = new List<OccurrenceFraction>();
            foreach (var compound in compounds) {
                var compoundMarginals = new Dictionary<string, MarginalOccurrencePattern>(StringComparer.OrdinalIgnoreCase);
                var sampleLocationFractions = sampleOrigins
                    .Select(r => new LocationOccurrenceFraction() {
                        Location = r.Location,
                        FractionAllSamples = r.Fraction,
                    })
                    .ToList();

                foreach (var sampleLocationFraction in sampleLocationFractions) {
                    var locationUses = agriculturalUses.FilterByLocation(sampleLocationFraction.Location);
                    foreach (var use in locationUses) {
                        if (!compoundMarginals.TryGetValue(use.Code, out var marginalUse)) {
                            marginalUse = new MarginalOccurrencePattern {
                                Food = food,
                                Code = use.Code,
                                Compounds = use.Compounds.ToHashSet(),
                                OccurrenceFraction = 0
                            };
                            compoundMarginals.Add(use.Code, marginalUse);
                        }
                        marginalUse.OccurrenceFraction += sampleLocationFraction.FractionAllSamples
                                                       * (use.OccurrenceFraction);
                        marginalUse.OccurrenceFraction = Math.Min(marginalUse.OccurrenceFraction, 1D);
                    }

                    // Compute the substance based agricultural use for this location
                    var compoundBasedAgriculturalUseForLocation = Math.Min(
                        locationUses.FilterByCompound(compound).Sum(cbau => cbau.OccurrenceFraction), 1D);

                    // If desired, add the unspecified part for this location
                    if (!_settings.SetMissingAgriculturalUseAsUnauthorized) {
                        var unspecifiedForLocation = Math.Max(1 - locationUses.Sum(cbau => cbau.OccurrenceFraction), 0D);
                        compoundBasedAgriculturalUseForLocation += unspecifiedForLocation;
                    }

                    // Add the substance based, location based agricultural use info to the dictionary
                    var substanceUseFound = compoundBasedAgriculturalUseForLocation > 0;

                    sampleLocationFraction.SubstanceUseFound = substanceUseFound;
                    sampleLocationFraction.OccurrenceFraction = compoundBasedAgriculturalUseForLocation;
                }

                // Compute the specified agricultural use for this compound
                var weightedAgriculturalUseFraction = compoundMarginals
                    .Where(r => r.Value.Compounds.Contains(compound))
                    .Sum(wau => wau.Value.OccurrenceFraction);
                weightedAgriculturalUseFraction = (weightedAgriculturalUseFraction > 1) ? 1D : weightedAgriculturalUseFraction;

                // If we want to include the unspecified part in the agricultural use percentage
                // then add it to the already specified agricultural use fraction
                if (!_settings.SetMissingAgriculturalUseAsUnauthorized) {
                    var unspecifiedFraction = 1 - compoundMarginals.Sum(au => au.Value.OccurrenceFraction);
                    unspecifiedFraction = (unspecifiedFraction < 0) ? 0D : unspecifiedFraction;
                    weightedAgriculturalUseFraction += unspecifiedFraction;
                }

                var compoundAgriculturalUseRecord = new OccurrenceFraction() {
                    Food = food,
                    Substance = compound,
                    LocationOccurrenceFractions = sampleLocationFractions.ToDictionary(r => r.Location ?? string.Empty, r => r as ILocationOccurrenceFrequency),
                    OccurrenceFrequency = weightedAgriculturalUseFraction,
                };
                records.Add(compoundAgriculturalUseRecord);
            }
            return records;
        }
    }
}
