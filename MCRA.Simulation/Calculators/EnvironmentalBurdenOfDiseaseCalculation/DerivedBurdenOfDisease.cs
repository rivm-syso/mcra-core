using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public class DerivedBurdenOfDisease : BurdenOfDisease {
        public HashSet<BodIndicatorConversion> Conversions { get; set; }
        public BurdenOfDisease SourceIndicator { get; set; }
    }
}
