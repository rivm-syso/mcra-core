using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Collects conversion info into argument object
    /// </summary>
    public sealed class FoodConversionResult {

        private string _allStepsToMeasuredString;

        public Food FoodAsEaten { get; set; }

        public Food FoodAsMeasured { get; set; }

        public Compound Compound { get; set; }

        public double Proportion { get; set; }

        public double MarketShare { get; set; }

        public List<FoodConversionResultStep> ConversionStepResults { get; set; } = new();

        public List<Food> FoodTrace { get; set; } = new();

        public string AllStepsToMeasuredString {
            get {
                if (string.IsNullOrEmpty(_allStepsToMeasuredString)) {
                    _allStepsToMeasuredString = string.Join("-", ConversionStepResults.Select(c => c.FoodCodeFrom).ToList());
                }
                return _allStepsToMeasuredString;
            }
        }

        public FoodConversionResult() {
            Proportion = 1;
            MarketShare = 1;
            ConversionStepResults = new List<FoodConversionResultStep>();
        }

        /// <summary>
        /// Creates a new FoodConversionResult with the same properties as the argument.
        /// </summary>
        /// <param name="fcr">The FoodConversionResult to copy</param>
        public FoodConversionResult(FoodConversionResult fcr) {
            FoodAsMeasured = fcr.FoodAsMeasured;
            FoodAsEaten = fcr.FoodAsEaten;
            Proportion = fcr.Proportion;
            MarketShare = fcr.MarketShare;
            Compound = fcr.Compound;
            ConversionStepResults.AddRange(fcr.ConversionStepResults);
            FoodTrace.AddRange(fcr.FoodTrace);
        }

        /// <summary>
        /// Initializes the current FoodConversionResult for a new search within the current foodaseaten.
        /// </summary>
        public void Initialize() {
            Proportion = 1;
            MarketShare = 1;
            ConversionStepResults = new List<FoodConversionResultStep>();
        }

        /// <summary>
        /// Gets the proportion associated with processing. ProportionProcessing is the conversion 
        /// step associated with processing.
        /// </summary>
        public double ProportionProcessing {
            get {
                return ConversionStepResults.LastOrDefault(r => r.ProcessingTypes?.Any() ?? false)?.Proportion ?? double.NaN;
            }
        }

        /// <summary>
        /// Processing types of the conversion.
        /// </summary>
        public ICollection<ProcessingType> ProcessingTypes {
            get {
                return ConversionStepResults.LastOrDefault(r => r.ProcessingTypes?.Any() ?? false)?.ProcessingTypes;
            }
        }

        /// <summary>
        /// Override of ToString function for debugging.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"From: {FoodAsEaten.Code}, To: {FoodAsMeasured?.Code}";
        }
    }
}
