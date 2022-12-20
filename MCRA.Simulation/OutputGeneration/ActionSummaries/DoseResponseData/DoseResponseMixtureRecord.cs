using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseMixtureRecord {
        public List<MixtureDose> MixtureDose { get; set; }
        public double Response { get; set; }
        public double SD { get; set; } = double.NaN;
        public int N { get; set; } = 0;
    }
}
