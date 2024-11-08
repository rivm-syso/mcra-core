namespace MCRA.Utils.Statistics.RandomGenerators {

    /// <summary>
    /// Wrapper around another generator. Can record and playback elements of the generated random sequence.
    /// All elements are stored as doubles, but are transformed to integers, booleans or byte arrays if needed.
    /// </summary>
    public class CapturingGenerator : IRandom {

        private readonly IRandom _generator;
        private readonly List<double> _capturedSequence = [];
        private bool _isCapturing = false;
        private bool _isRepeating = false;
        private IEnumerator<double> _sequenceEnumerator;

        /// <summary>
        /// Initializes a new instance of this object.
        /// </summary>
        /// <param name="generator">The generator for which record and playback functionality is required.</param>
        public CapturingGenerator(IRandom generator) {
            _generator = generator;
        }

        /// <summary>
        /// Gets whether random numbers are currently being captured.
        /// </summary>
        public bool IsCapturing {
            get { return !_isRepeating && _isCapturing; }
        }

        /// <summary>
        /// Gets whether random numbers are currently being played back from a captured sequence.
        /// </summary>
        public bool IsRepeating {
            get { return _isRepeating; }
        }

        public int Seed => _generator.Seed;

        public bool CanReset => true;

        /// <summary>
        /// Draws a random integer number between minValue and maxValue, by drawing a double from the wrapped Generator
        /// and converting it to an integer. Including min value, excluding max value
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxValue) {
            return (int)Math.Floor(NextDouble() * (maxValue - minValue) + minValue);
        }

        /// <summary>
        /// Draws a random integer number between 0 and maxValue, by drawing a double from the wrapped Generator
        /// and conveting it to an integer.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Next(int maxValue) {
            return (int)Math.Floor(NextDouble() * maxValue);
        }

        /// <summary>
        /// Draws a random integer number between 0 and int.MaxValue, by drawing a double from the wrapped Generator
        /// and converting it to an integer.
        /// </summary>
        /// <returns></returns>
        public int Next() {
            return (int)Math.Floor(NextDouble() * int.MaxValue);
        }

        /// <summary>
        /// Draws a random boolean value by drawing a double from the wrapped Generator and converting it to a double.
        /// </summary>
        /// <returns></returns>
        public bool NextBoolean() {
            return NextDouble() > 0.5;
        }


        /// <summary>
        /// Draws a random double value between 0 and maxValue
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public double NextDouble(double maxValue) {
            return NextDouble() * maxValue;
        }

        /// <summary>
        /// Draws a random double value between 0 and 1
        /// </summary>
        /// <returns></returns>
        public double NextDouble() {
            if (_isRepeating) {
                if (_sequenceEnumerator.MoveNext()) {
                    return _sequenceEnumerator.Current;
                } else {
                    _isRepeating = false;
                    _sequenceEnumerator = null;
                    return NextDouble();
                }

            } else {
                var number = _generator.NextDouble();
                if (IsCapturing) {
                    _capturedSequence.Add(number);
                }
                return number;
            }
        }

        /// <summary>
        /// Clears the currently captured sequence and resets the wrapped Generator if possible.
        /// </summary>
        /// <returns></returns>
        public void Reset() {
            if (_generator.CanReset) {
                _generator.Reset();
            }
            _capturedSequence.Clear();
        }

        /// <summary>
        /// Starts the capturing process. All numbers generated from now on will be stored.
        /// </summary>
        public void StartCapturing() {
            _isCapturing = true;
        }

        /// <summary>
        /// Stops the capturing process.
        /// </summary>
        public void StopCapturing() {
            _isCapturing = false;
        }

        /// <summary>
        /// Playback the captured sequence when a random number is drawn. When the recorded sequence has been played back,
        /// numbers are generated and stored as usual. (They are stored depending on whether 'StopCapturing()' has been invoked.
        /// </summary>
        public void Repeat() {
            if (_capturedSequence.Count > 0) {
                _isRepeating = true;
                _sequenceEnumerator = _capturedSequence.GetEnumerator();
            }
        }

        public double NextDouble(double minValue, double maxValue)
            => NextDouble() * (maxValue - minValue) + minValue;
    }
}
