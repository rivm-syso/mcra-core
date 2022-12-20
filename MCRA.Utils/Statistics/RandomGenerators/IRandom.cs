namespace MCRA.Utils.Statistics {
    public interface IRandom {
        int Seed { get; }

        /// <summary>
        /// Returns a non-negative random integer. A 32-bit signed integer that is 
        /// greater than or equal to 0 and less than System.Int32.MaxValue.
        /// </summary>
        /// <returns></returns>
        int Next();

        /// <summary>
        /// Returns a signed integer greater than or equal to 0 and less than maxValue.
        /// I.e., the range of return values includes 0 but not maxValue.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        int Next(int maxValue);

        /// <summary>
        /// Returns a signed integer greater than or equal to minValue and less than maxValue.
        /// I.e., the range of return values includes minValue but not maxValue.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Returns a random double that is greater than or equal to 0.0 and less than 1.0.
        /// </summary>
        /// <returns></returns>
        double NextDouble();

        /// <summary>
        /// A double greater than or equal to minValue, and less than maxValue. I.e., the 
        /// range of return values includes minValue but not maxValue.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        double NextDouble(double minValue, double maxValue);

        /// <summary>
        /// Specifies whether the random number generator can reset.
        /// </summary>
        bool CanReset { get; }

        /// <summary>
        /// Resets the random number generator to its initial state (with its initial seed).
        /// </summary>
        void Reset();
    }
}
