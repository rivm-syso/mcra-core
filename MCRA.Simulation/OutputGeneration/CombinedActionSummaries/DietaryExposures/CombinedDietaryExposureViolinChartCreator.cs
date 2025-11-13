using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public class CombinedDietaryExposureViolinChartCreator(
       CombinedDietaryExposurePercentilesSection section,
       double percentile,
       bool horizontal,
       bool boxplotItem,
       bool equalSize
    ) : CombinedViolinChartCreatorBase(section, percentile, horizontal, boxplotItem, equalSize) {

        public ExternalExposureUnit ExposureUnit => section.ExposureUnit;

        public override string ChartId {
            get {
                var pictureId = "bbde56ae-040d-499d-a320-dc5a1a113cb5";
                return StringExtensions.CreateFingerprint(_section.SectionId + _percentile + pictureId);
            }
        }

        public override string Title {
            get {
                return $"Violin plots of the uncertainty distribution of the dietary exposures at the" +
                    $" p{_percentile} percentile of the population exposure distributions. The vertical lines " +
                    $"represent the median and the lower p{_lowerBound} and upper p{_upperBound} bound of the " +
                    $"uncertainty distribution. The nominal run is indicated by the black dot.";
            }
        }

        protected override string HorizontalAxisTitle => $"Dietary exposure ({ExposureUnit.GetShortDisplayName()})";
    }
}
