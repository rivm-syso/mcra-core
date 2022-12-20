
namespace MCRA.Data.Compiled.Objects {
    public sealed class MarketShare {
        public Food Food { get; set; }
        public double Percentage { get; set; }
        public double BrandLoyalty { get; set; }
        public override string ToString() {
            return $"[{GetHashCode():X8}] {Percentage}";
        }
    }
}
