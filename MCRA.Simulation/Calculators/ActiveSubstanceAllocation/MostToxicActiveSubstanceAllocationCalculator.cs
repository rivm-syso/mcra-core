using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {
    public sealed class MostToxicActiveSubstanceAllocationCalculator : ActiveSubstanceAllocationCalculatorBase {

        private IDictionary<Compound, double> _relativePotencyFactors { get; set; }

        public MostToxicActiveSubstanceAllocationCalculator(
            ICollection<SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            bool useSubstanceAuthorisations,
            bool retainAllAllocatedSubstancesAfterAllocation,
            IDictionary<Compound, double> relativePotencyFactors,
            bool tryFixDuplicateAllocationInconsistencies = false
        ) : base(substanceConversions, substanceAuthorisations, useSubstanceAuthorisations, retainAllAllocatedSubstancesAfterAllocation, tryFixDuplicateAllocationInconsistencies) {
            _relativePotencyFactors = relativePotencyFactors;
        }

        protected override ActiveSubstanceConversionRecord convert(
            SampleCompound sampleCompound,
            Food food,
            SubstanceConversionCollection substanceTranslationCollection,
            IRandom random
        ) {

            // Obtain substances from translation collection and authorisations from authorised uses
            var substances = substanceTranslationCollection.LinkedActiveSubstances
                .Select(r => (
                    Substance: r.Key,
                    Authorised: !_useSubstanceAuthorisations
                        || (_substanceAuthorisations?.ContainsKey((food, r.Key)) ?? true)
                        || (food.BaseFood != null && (_substanceAuthorisations?.ContainsKey((food.BaseFood, r.Key)) ?? true))
                ))
                .ToList();

            if (substances.Any(r => r.Authorised)) {
                // Filter by authorised substances (if there is at least one authorised substance)
                substances = substances.Where(r => r.Authorised).ToList();
            }

            // Select most toxic substance
            var mostToxicSubstance = substances
                .OrderByDescending(r => _relativePotencyFactors.TryGetValue(r.Substance, out var rpf) ? rpf : 0D)
                .ThenBy(r => r.Substance.Name)
                .ThenBy(r => r.Substance.Code)
                .First().Substance;

            // Create a new record for each active substance in the translation collection
            var result = new List<SampleCompound>();
            foreach (var activeSubstance in substanceTranslationCollection.LinkedActiveSubstances) {
                // Determine multiplication factor based on the drawn translation set
                var factor = activeSubstance.Key == mostToxicSubstance ? activeSubstance.Value : 0D;
                var resType = sampleCompound.IsCensoredValue && factor == 0
                    ? ResType.VAL : sampleCompound.ResType;
                var record = new SampleCompound() {
                    ActiveSubstance = activeSubstance.Key,
                    MeasuredSubstance = sampleCompound.MeasuredSubstance,
                    ResType = resType,
                    Lod = sampleCompound.Lod * factor,
                    Loq = sampleCompound.Loq * factor,
                    // Note: if the factor is zero, then the measurement is assumed to be a zero
                    // and a zero should be assigned to the residue field
                    Residue = sampleCompound.IsCensoredValue
                        ? (factor == 0 ? 0 : double.NaN)
                        : factor * sampleCompound.Residue
                };
                result.Add(record);
            }

            // Return allocated active substance records
            return new ActiveSubstanceConversionRecord() {
                MeasuredSubstanceSampleCompound = sampleCompound,
                ActiveSubstanceSampleCompounds = result,
                Authorised = !sampleCompound.IsPositiveResidue
                    || (_substanceAuthorisations?.ContainsKey((food, mostToxicSubstance)) ?? true)
                    || (food.BaseFood != null && (_substanceAuthorisations?.ContainsKey((food.BaseFood, mostToxicSubstance)) ?? true))
            };
        }
    }
}
