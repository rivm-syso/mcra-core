namespace MCRA.Data.Compiled.Objects {
    public sealed class TDSFoodSampleComposition {
        public Food TDSFood { get; set; }
        public Food Food { get; set; }
        public double PooledAmount { get; set; }
        public string Regionality { get; set; }
        public string Seasonality { get; set; }
        public string Description { get; set; }
    }
}
