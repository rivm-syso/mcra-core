using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Acute summary record for a substance
    /// </summary>
    public sealed class DietaryAcuteIntakePerCompoundRecord {
        public string CompoundCode { get; set; }
        public string CompoundName { get; set; }
        public double Concentration { get; set; }
        public double ProcessingFactor { get; set; }
        public double ProportionProcessing { get; set; }
        public double UnitWeight { get; set; }
        public List<IntakePortion> UnitVariabilityPortions { get; set; }
        public double UnitVariabilityFactor { get; set; }
        public double CoefficientOfVariation { get; set; }
        public double UnitsInCompositeSample { get; set; }
        public string ProcessingTypeDescription { get; set; }
        public double Intake { get; set; }
        public double Rpf { get; set; }

        public DietaryAcuteIntakePerCompoundRecord() {
            Rpf = double.NaN;
        }
    }
}
