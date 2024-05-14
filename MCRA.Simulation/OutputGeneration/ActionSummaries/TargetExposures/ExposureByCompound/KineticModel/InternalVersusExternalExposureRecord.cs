using MCRA.General;
using MCRA.Utils.Collections;

namespace MCRA.Simulation.OutputGeneration {
    public class InternalVersusExternalExposureRecord {

        public double ExternalExposure { get; set; }

        public SerializableDictionary<ExposureTarget, double> TargetExposure { get; set; }

    }
}
