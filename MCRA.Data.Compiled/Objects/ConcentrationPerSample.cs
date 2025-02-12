﻿using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConcentrationPerSample {
        public SampleAnalysis Sample { get; set; }
        public Compound Compound { get; set; }
        public double? Concentration { get; set; }

        public ResType ResType { get; set; } = ResType.VAL;
    }
}
