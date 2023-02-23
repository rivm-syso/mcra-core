using MCRA.General;

namespace MCRA.Simulation.Action.UncertaintyFactorial {

    /// <summary>
    /// Class represents the result of a factorial run.
    /// </summary>
    public sealed class UncertaintyFactorialResultRecord {

        /// <summary>
        /// Creates a new <see cref="UncertaintyFactorialResultRecord"/> instance.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="resultRecord"></param>
        public UncertaintyFactorialResultRecord(
            List<UncertaintySource> tags,
            IUncertaintyFactorialResult resultRecord
        ) {
            Tags = tags;
            ResultRecord = resultRecord;
        }

        /// <summary>
        /// The tag composed of the uncertainty sources used to generate the
        /// result record.
        /// </summary>
        public List<UncertaintySource> Tags { get; set; }

        /// <summary>
        /// The factorial result record.
        /// </summary>
        public IUncertaintyFactorialResult ResultRecord { get; set; }
    }
}
