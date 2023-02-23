namespace MCRA.Utils.Statistics {
    public class RandomAsRandomWrapper : Random {


        private readonly IRandom _random;
        public RandomAsRandomWrapper(IRandom random) {
            _random = random;
        }
        public override int Next() => _random.Next();

        public override int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

        public override int Next(int maxValue) => _random.Next(maxValue);

        //public override void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

        public override double NextDouble() => _random.NextDouble();
    }
}
