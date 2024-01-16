using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public class SingleValueDietaryExposureSource : IExposureSource {

        public ISingleValueDietaryExposure Source { get; set; }

        public ExposurePathType Route { get; set; }

        public string Code {
            get {
                if (Source.ProcessingType != null) {
                    return $"{Source.Food.Code}-{Source.ProcessingType.Code}";
                } else {
                    return Source.Food.Code;
                }
            }
        }

        public string Name {
            get {
                if (Source.ProcessingType != null) {
                    return $"{Source.Food.Name} ({Source.ProcessingType.Name})";
                } else {
                    return Source.Food.Name;
                }
            }
        }
    }
}
