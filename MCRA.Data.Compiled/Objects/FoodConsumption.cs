namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodConsumption {
        private double _amount { get; set; }

        public string idMeal { get; set; }

        public string idDay => IndividualDay.IdDay;

        public double Amount {
            get {
                return _amount;
            }
            set {
                if (value <= 0) {
                    throw new Exception("Consumption amount cannot be smaller than or equal to zero.");
                }
                _amount = value;
            }
        }
        public DateTime? DateConsumed { get; set; }

        public FoodConsumptionQuantification FoodConsumptionQuantification { get; set; }

        public Food Food { get; set; }

        public ICollection<FoodFacet> FoodFacets { get; set; }

        public Individual Individual => IndividualDay.Individual;

        public IndividualDay IndividualDay { get; set; }
    }
}
