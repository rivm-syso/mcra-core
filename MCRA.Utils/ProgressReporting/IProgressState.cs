using System;
using System.Threading;

namespace MCRA.Utils.ProgressReporting {

    /// <summary>
    /// Delegate function for handling progress state changed events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ProgressStateChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Interface for a progress state that can be used for progress reporting
    /// </summary>
    public interface IProgressState {

        /// <summary>
        /// Holds a string with a description of the current activity
        /// </summary>
        string CurrentActivity { get; set; }

        /// <summary>
        /// Hold a double (which should be between 0 and 100) of the progress percentage
        /// </summary>
        double Progress { get; }

        /// <summary>
        /// A getter for the cancellation token
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Fires when 'CurrentActivity' is changed
        /// </summary>
        event ProgressStateChangedEventHandler CurrentActivityChanged;

        /// <summary>
        /// Fires when 'Progress' is changed
        /// </summary>
        event ProgressStateChangedEventHandler ProgressStateChanged;

        void MarkCompleted();
    }

}
