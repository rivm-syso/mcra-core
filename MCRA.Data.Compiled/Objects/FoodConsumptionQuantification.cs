namespace MCRA.Data.Compiled.Objects {

    public sealed class FoodConsumptionQuantification {
        public Food Food { get; set; }

        public string UnitCode { get; set; }
        public double UnitWeight { get; set; }
        public double? UnitWeightUncertainty { get; set; }
        public double? AmountUncertainty { get; set; }
    }
}
