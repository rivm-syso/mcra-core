namespace MCRA.Utils.Statistics {
    public class McraRandomGenerator : Random, IRandom {

        private Random _generator;
        private readonly int _seed;

        public McraRandomGenerator() {
            _generator = new Random();
        }
        public McraRandomGenerator(int seed, bool obsoleteParameter = false) {
            _seed = seed;
            _generator = new Random(_seed);
        }

        /// <summary>
        /// Only implemented for unit tests sofar
        /// </summary>
        /// <param name="seeds"></param>
        public McraRandomGenerator(int[] seeds) {
            if (seeds == null || !seeds.Any()) {
                var rnd = new Random();
                _seed = rnd.Next();
                _generator = new Random(_seed);
            } else if (seeds.Length == 1) {
                _seed = seeds[0];
                _generator = new Random(_seed);
            } else {
                _seed = seeds.Length;
                foreach (int val in seeds) {
                    _seed = unchecked(_seed * 314159 + val);
                }
                _generator = new Random(_seed);
            }
        }

        public int Seed => _seed;

        public bool CanReset => true;

        public override int Next() => _generator.Next();

        public override int Next(int maxValue) => _generator.Next(maxValue);

        public override int Next(int minValue, int maxValue) => _generator.Next(minValue, maxValue);

        public override double NextDouble() => _generator.NextDouble();

        public double NextDouble(double minValue, double maxValue) {
            return _generator.NextDouble() * (maxValue - minValue) + minValue;
        }

        public void Reset() {
            _generator = new Random(_seed);
        }
    }
}
