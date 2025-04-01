namespace MCRA.Simulation.Objects {

    public sealed class ConvertedSingleValueConcentrationModel : SingleValueConcentrationModel {

        /// <summary>
        /// The conversion factor.
        /// </summary>
        public double ConversionFactor { get; set; }

        /// <summary>
        /// The underlying "measured substance" single value concentration.
        /// </summary>
        public SingleValueConcentrationModel MeasuredSingleValueConcentrationModel { get; set; }
    }
}
