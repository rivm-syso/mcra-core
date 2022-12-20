using MCRA.Utils.Collections;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class DetailCoExposureRecord : FullCoExposureRecord {
        public BitPattern32 Binary { get; set; }
        public int[] Index { get; set; }
        public int Row { get; set; }
    }
}
