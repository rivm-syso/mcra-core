namespace MCRA.Data.Compiled.Objects {
    using System.Collections.Generic;

    public sealed class RelativePotencyFactor {
        public RelativePotencyFactor() {
            this.RelativePotencyFactorsUncertains = new HashSet<RelativePotencyFactorUncertain>();
        }

        public double RPF { get; set; }

        public Compound Compound { get; set; }
        public Effect Effect { get; set; }
        public ICollection<RelativePotencyFactorUncertain> RelativePotencyFactorsUncertains { get; set; }

    }
}
