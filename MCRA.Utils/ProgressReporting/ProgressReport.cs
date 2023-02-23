namespace MCRA.Utils.ProgressReporting {

    /// <summary>
    /// A CompositeProgressState with an additional Overall Status Message property.
    /// </summary>
    public sealed class ProgressReport : CompositeProgressState {

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ProgressReport() {
        }

        /// <summary>
        /// Constructor with cancellation token for monitoring the cancel status
        /// of the process.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public ProgressReport(CancellationToken cancellationToken) : base(cancellationToken) {
        }
    }
}
