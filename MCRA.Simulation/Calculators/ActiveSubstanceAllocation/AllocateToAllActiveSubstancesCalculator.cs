using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.SubstanceConversionsCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {
    public sealed class AllocateToAllActiveSubstancesCalculator : ActiveSubstanceAllocationCalculatorBase {

        public AllocateToAllActiveSubstancesCalculator(
            ICollection<SubstanceConversion> residueDefinitions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            bool useSubstanceAuthorisations,
            bool retainAllAllocatedSubstancesAfterAllocation,
            bool tryFixDuplicateAllocationInconsistencies = false
        ) : base(residueDefinitions, substanceAuthorisations, useSubstanceAuthorisations, retainAllAllocatedSubstancesAfterAllocation, tryFixDuplicateAllocationInconsistencies) {
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
                    Authorised: !_useSubstanceAuthorisations || (_substanceAuthorisations?.ContainsKey((food, r.Key)) ?? true)
                ))
                .ToList();

            if (substances.Any(r => r.Authorised)) {
                // Filter by authorised substances (if there is at least one authorised substance)
                substances = substances.Where(r => r.Authorised).ToList();
            }

            // Create a new record for each active substance in the translation collection
            var result = new List<SampleCompound>();
            foreach (var activeSubstance in substanceTranslationCollection.LinkedActiveSubstances) {
                // Determine multiplication factor based on the drawn translation set
                var factor = activeSubstance.Value;
                var resType = sampleCompound.IsCensoredValue && factor == 0 
                    ? ResType.VAL : sampleCompound.ResType;
                var record = new SampleCompound() {
                    ActiveSubstance = activeSubstance.Key,
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
                Authorised = !sampleCompound.IsPositiveResidue || substances.All(s => s.Authorised)
            };
        }
    }
}
