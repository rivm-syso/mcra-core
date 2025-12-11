using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BodIndicatorModels {
    public class DerivedBodIndicatorModel : IBodIndicatorModel {

        public HashSet<BodIndicatorConversion> Conversions { get; set; }

        public IBodIndicatorModel SourceIndicator { get; set; }

        public BurdenOfDisease BurdenOfDisease { get; set; }

        public Population Population => BurdenOfDisease.Population;

        public Effect Effect => BurdenOfDisease.Effect;

        public BodIndicator BodIndicator => BurdenOfDisease.BodIndicator;

        public double GetBodIndicatorValue() {
            var conversionFactor = 1D;
            foreach (var conversion in Conversions) {
                conversionFactor *= conversion.Value;
            }
            return conversionFactor * SourceIndicator.GetBodIndicatorValue();
        }

        public void ResampleModelParameters(IRandom random) {
            // Do nothing: assume source model is resampled
        }
    }
}
