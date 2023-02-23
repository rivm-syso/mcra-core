using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// This class holds a collection of sample compound records for a specified food.
    /// </summary>
    public sealed class SampleCompoundCollection {

        /// <summary>
        /// The food
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// Get the sample compound records
        /// </summary>
        public List<SampleCompoundRecord> SampleCompoundRecords { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleCompoundCollection" /> class.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="sampleCompoundRecords"></param>
        public SampleCompoundCollection(Food food, List<SampleCompoundRecord> sampleCompoundRecords) {
            Food = food;
            SampleCompoundRecords = sampleCompoundRecords;
        }

        public override int GetHashCode() {
            return Food.Code.GetChecksum();
        }

        public SampleCompoundCollection Clone() {
            var records = SampleCompoundRecords
                .AsParallel()
                .Select(scr => scr.Clone())
                .ToList();
            return new SampleCompoundCollection(this.Food, records);
        }
    }
}
