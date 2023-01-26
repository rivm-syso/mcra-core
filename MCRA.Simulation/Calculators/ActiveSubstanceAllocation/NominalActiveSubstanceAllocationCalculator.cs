using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {
    public sealed class NominalActiveSubstanceAllocationCalculator : ActiveSubstanceAllocationCalculatorBase {

        public NominalActiveSubstanceAllocationCalculator(
            ICollection<SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            bool useSubstanceAuthorisations,
            bool retainAllAllocatedSubstancesAfterAllocation,
            bool tryFixDuplicateAllocationInconsistencies = false
        ) : base(substanceConversions, substanceAuthorisations, useSubstanceAuthorisations, retainAllAllocatedSubstancesAfterAllocation, tryFixDuplicateAllocationInconsistencies) {
        }

        protected override ActiveSubstanceConversionRecord convert(
            SampleCompound sampleCompound,
            Food food,
            SubstanceConversionCollection substanceTranslationCollection,
            IRandom random
        ) {
            // Get possible translation sets
            var translationSets = substanceTranslationCollection.SubstanceTranslationSets;

            // Create a new record for each active substance in the translation collection
            var result = new List<SampleCompound>();
            foreach (var activeSubstance in substanceTranslationCollection.LinkedActiveSubstances.Keys) {
                // The multiplication factor for this substance is the weighted sum of its conversion factors per translation set
                var factor = translationSets.Sum(r => r.PositiveSubstanceConversions.ContainsKey(activeSubstance) ? r.TranslationProportion * r.PositiveSubstanceConversions[activeSubstance] : 0D);
                var resType = sampleCompound.IsCensoredValue && factor == 0
                    ? ResType.VAL : sampleCompound.ResType;
                var record = new SampleCompound() {
                    ActiveSubstance = activeSubstance,
                    MeasuredSubstance = sampleCompound.MeasuredSubstance,
                    ResType = resType,
                    Loq = sampleCompound.Loq * factor,
                    Lod = sampleCompound.Lod * factor,
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
                    || substanceTranslationCollection.LinkedActiveSubstances.Keys
                        .Any(r => _substanceAuthorisations.ContainsKey((food, r))
                            || (food.BaseFood != null && _substanceAuthorisations.ContainsKey((food.BaseFood, r)))
                        )
            };
        }
    }
}
