namespace MCRA.Data.Compiled.Objects {
    public sealed class FoodTranslation {

        public FoodTranslation() { }

        public FoodTranslation(Food foodFrom, Food foodTo, double proportion) {
            FoodFrom = foodFrom;
            FoodTo = foodTo;
            Proportion = proportion;
        }

        public Food FoodFrom { get; set; }
        public Food FoodTo { get; set; }
        public double Proportion { get; set; }
        public string IdPopulation { get; set; }
    }
}
