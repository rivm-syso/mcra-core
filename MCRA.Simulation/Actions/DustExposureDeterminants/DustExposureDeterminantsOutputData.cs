﻿using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DustExposureDeterminants {
    public class DustExposureDeterminantsOutputData : IModuleOutputData {
        public IList<DustIngestion> DustIngestions { get; set; }
        public IList<DustBodyExposureFraction> DustBodyExposureFractions { get; set; }
        public IList<DustAdherenceAmount> DustAdherenceAmounts { get; set; }
        public IList<DustAvailabilityFraction> DustAvailabilityFractions { get; set; }

        public IModuleOutputData Copy() {
            return new DustExposureDeterminantsOutputData() {
                DustIngestions = DustIngestions,
                DustBodyExposureFractions = DustBodyExposureFractions,
                DustAdherenceAmounts = DustAdherenceAmounts,
                DustAvailabilityFractions = DustAvailabilityFractions
            };
        }
    }
}
