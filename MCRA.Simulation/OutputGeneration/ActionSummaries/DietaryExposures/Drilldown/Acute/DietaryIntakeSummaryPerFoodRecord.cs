
namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryIntakeSummaryPerFoodRecord {
        public string FoodName { get; set; }
        public string FoodCode { get; set; }
        public double Concentration { get; set; }
        public double IntakePerMassUnit { get; set; }
        public double AmountConsumed { get; set; }
        public double GrossAmountConsumed { get; set; }
        public override string ToString() {
            return $"{FoodName}: {AmountConsumed}";
        }
    }
}
