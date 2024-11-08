namespace MCRA.Utils.ProgressReporting {

    /// <summary>
    /// A progressState that consists of other sub-progress states. Use this class when reporting the progress of a large process that
    /// consists of other sub-processes. When adding a new sub-progress, a percentage should be given that indicites how much the added sup-progress
    /// adds to the total progress.
    /// </summary>
    public class CompositeProgressState : IProgressState {

        private string _currentActivity = string.Empty;
        private readonly Dictionary<IProgressState, double> _subProgressPercentageOfTotals = [];
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes the composite progress state.
        /// </summary>
        public CompositeProgressState()
            : this(default(CancellationToken)){
        }

        /// <summary>
        /// Initializes the composite progress state without a cancellation token.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public CompositeProgressState(CancellationToken cancellationToken) {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Fires when the progress state value has changed
        /// </summary>
        public event ProgressStateChangedEventHandler ProgressStateChanged;

        /// <summary>
        /// Fires when the current activity message has changed
        /// </summary>
        public event ProgressStateChangedEventHandler CurrentActivityChanged;

        /// <summary>
        /// The cancellation token is used for checking whether the progress is cancelled.
        /// If the process is cancelled, then a cancel-error is thrown.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationToken;

        /// <summary>
        /// Action on change of current activity
        /// </summary>
        private void OnCurrentActivityChanged() {
            // Throw the cancellation error is the cancel request has been fired
            if (_cancellationToken.IsCancellationRequested) {
                _cancellationToken.ThrowIfCancellationRequested();
            }

            CurrentActivityChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Current activity message of the most recently active sub progress
        /// </summary>
        public string CurrentActivity {
            get { return _currentActivity; }
            set {_currentActivity = value;
                OnCurrentActivityChanged();
            }
        }

        /// <summary>
        /// The total progress of all sub progresses, scaled with the factor they where given when instantiated.
        /// </summary>
        public double Progress => _subProgressPercentageOfTotals.Aggregate(0D, (total, next) => total + next.Key.Progress * next.Value / 100);

        /// <summary>
        /// All subprogress states
        /// </summary>
        public IEnumerable<IProgressState> SubProgressStates => _subProgressPercentageOfTotals.Keys.AsEnumerable();

        /// <summary>
        /// Adds a new sub-ProgressState and returns it.
        /// </summary>
        /// <param name="percentageOfTotal">The amount the new subprogress adds to the total progress</param>
        /// <returns>The newly created subprogress</returns>
        /// <example>
        /// var cp = new CompositeProgress()
        /// var subProgress1 = cp.NewProgressState(25); //subprogress that accounts for 25% of total
        /// var subProgress2 = cp.NewProgressState(75); //subprogress
        ///
        /// subProgress1.Progress = 50;
        /// cp.Progress == 12.5 //true
        ///
        /// subProgress2.Progress = 100;
        /// cp.Progress == 87.5 //true
        ///
        /// subProgress1.Progress = 100;
        /// cp.Progress == 100 //true
        /// </example>
        public ProgressState NewProgressState(double percentageOfTotal) {
            var subState = new ProgressState(_cancellationToken);
            subState.CurrentActivityChanged += OnSubProgressActivityChanged;
            subState.ProgressStateChanged += OnSubProgressStateChanged;
            _subProgressPercentageOfTotals.Add(subState, percentageOfTotal);
            return subState;
        }

        /// <summary>
        /// Adds a new composite sub-progress and returns it.
        /// </summary>
        /// <param name="percentageOfTotal">The amount the new subprogress adds to the total progress</param>
        /// <returns></returns>
        public CompositeProgressState NewCompositeState(double percentageOfTotal) {
            var subState = new CompositeProgressState(_cancellationToken);
            subState.CurrentActivityChanged += OnSubProgressActivityChanged;
            subState.ProgressStateChanged += OnSubProgressStateChanged;
            _subProgressPercentageOfTotals.Add(subState, percentageOfTotal);
            return subState;
        }

        /// <summary>
        /// Marks this progress state as completed.
        /// </summary>
        public void MarkCompleted() {
            var remainingProgress = 100D - _subProgressPercentageOfTotals.Sum(r => r.Value);
            if (remainingProgress > 0) {
                var x = NewProgressState(remainingProgress);
            }
            foreach (var sub in SubProgressStates) {
                sub.MarkCompleted();
            }
        }

        /// <summary>
        /// Handles a sub-progress activity changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubProgressActivityChanged(object sender, EventArgs e) {
            var subProgressState = (IProgressState)sender;
            CurrentActivity = subProgressState.CurrentActivity;
        }

        /// <summary>
        /// Handles a sub-progress state changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubProgressStateChanged(object sender, EventArgs e) {
            if (ProgressStateChanged != null) {
                ProgressStateChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Returns the progres state percentage
        /// Put in first argument the number of unconditional progressstates,
        /// Put in params argument all conditional bools [in if statements]
        /// </summary>
        /// <param name="unconditional"></param>
        /// <param name="conditionalBools"></param>
        /// <returns></returns>
        public double GetProgressStatePercentage(int unconditional, params bool[] conditionalBools) {
            return GetProgressStatePercentage(0, unconditional, conditionalBools);
        }

        /// <summary>
        /// Returns the progres state percentage
        /// Put in first argument the percentage already used, then the number of unconditional progressstates,
        /// Put in params argument all conditional bools [in if statements]
        /// </summary>
        /// <param name="p"></param>
        /// <param name="unconditional"></param>
        /// <param name="conditionalBools"></param>
        /// <returns></returns>
        public double GetProgressStatePercentage(double p, int unconditional, params bool[] conditionalBools) {
            var count = conditionalBools.Count(c => c == true) + unconditional;
            return (100D - p) / count;
        }
    }
}
