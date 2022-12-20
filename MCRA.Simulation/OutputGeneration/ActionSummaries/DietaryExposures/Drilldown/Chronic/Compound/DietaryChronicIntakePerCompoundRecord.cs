namespace MCRA.Simulation.OutputGeneration {

    public sealed class DietaryChronicIntakePerCompoundRecord {
        public string CompoundCode { get; set; }
        public string CompoundName { get; set; }
        public double Concentration { get; set; }
        public double ProcessingFactor { get; set; }
        public double ProportionProcessing { get; set; }
        public double Intake { get; set; }
        public double Rpf { get; set; }

        public DietaryChronicIntakePerCompoundRecord() {
            Rpf = double.NaN;
        }
    }
}
