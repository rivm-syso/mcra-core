﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public class SoilMatchedExposureGenerator : SoilExposureGenerator {

        protected override SoilIndividualDayExposure createSoilIndividualExposure(
            IIndividualDay individualDay,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        ) {
            var soilIndividualExposures = soilIndividualDayExposures
                .FirstOrDefault(r => r.SimulatedIndividual.Id == individualDay.SimulatedIndividual.Id);
            if (soilIndividualExposures == null) {
                var msg = $"Failed to find matching soil exposure for individual [{individualDay.SimulatedIndividual.Code}].";
                throw new Exception(msg);
            }
            var result = soilIndividualExposures.Clone();
            return soilIndividualExposures;
        }
    }
}

