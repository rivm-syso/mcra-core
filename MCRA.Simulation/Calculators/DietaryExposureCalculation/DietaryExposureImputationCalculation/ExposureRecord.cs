namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation {

    public sealed class ExposureRecord {
        public int IndividualDayId { get; set; }
        public double Exposure { get; set; }
        public double BodyWeight { get; set; }
        public double SamplingWeight { get; set; }

        public double ExposurePerBodyWeight{
            get {
                return Exposure / BodyWeight;
            }
        }
    }
}
