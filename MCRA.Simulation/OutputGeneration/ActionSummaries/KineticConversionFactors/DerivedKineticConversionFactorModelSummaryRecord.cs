using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public struct ExposurePair {
        public double ExposureFrom { get; set; }
        public double ExposureTo { get; set; }

        public ExposurePair() { }

        public ExposurePair(double exposureFrom, double exposureTo) {
            ExposureFrom = exposureFrom;
            ExposureTo = exposureTo;
        }
    }

    public class DerivedKineticConversionFactorModelSummaryRecord : KineticConversionFactorModelSummaryRecord {
        [Display(AutoGenerateField = false)]
        public List<ExposurePair> ExposurePairs { get; set; }
    }
}
