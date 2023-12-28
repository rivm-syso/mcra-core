using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {
    [RawDataSourceTableID(RawDataSourceTableID.FoodTranslations)]
    public sealed class RawFoodTranslation : IRawDataTableRecord {
        public string idFromFood { get; set; }
        public string idToFood { get; set; }
        public double Proportion { get; set; }
        public string idPopulation { get; set; }
    }
}
