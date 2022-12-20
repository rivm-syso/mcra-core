using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Contains information about a sample, such as the analytical method
    /// used for the sample, the concentration information per substance, and
    /// the (imputed) relative potency.
    /// </summary>
    public sealed class SampleCompoundRecord {

        /// <summary>
        /// The sample record
        /// </summary>
        public FoodSample FoodSample { get; set; }

        /// <summary>
        /// The sample information per substance
        /// </summary>
        public Dictionary<Compound, SampleCompound> SampleCompounds { get; set; }

        /// <summary>
        /// Bool indicating whether this sample is an expected outcome from authorised use.
        /// </summary>
        public bool AuthorisedUse { get; set; }

        /// <summary>
        /// EFSA name for sum(residue * RPF) for each sample
        /// </summary>
        public double ImputedCumulativePotency(IDictionary<Compound, double> correctedRpfs) {
            return SampleCompounds
                .Where(r => correctedRpfs.ContainsKey(r.Key) && r.Value.IsPositiveResidue)
                .Sum(r => correctedRpfs[r.Key] * r.Value.Residue);
        }

        /// <summary>
        /// Returns a copy/clone of the sample compound record.
        /// </summary>
        /// <returns></returns>
        public SampleCompoundRecord Clone() {
            return new SampleCompoundRecord() {
                FoodSample = FoodSample,
                AuthorisedUse = AuthorisedUse,
                SampleCompounds = SampleCompounds.Values.ToDictionary(sc => sc.ActiveSubstance, sc => sc.Clone())
            };
        }
    }
}
