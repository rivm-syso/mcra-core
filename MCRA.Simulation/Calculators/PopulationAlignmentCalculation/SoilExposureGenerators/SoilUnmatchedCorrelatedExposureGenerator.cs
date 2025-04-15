using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.SoilExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.SoilExposureGenerators {

    public class SoilUnmatchedCorrelatedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and dietary individuals
        /// (if the properties of the dietary individual match the properties of the soil individual)
        /// </summary>
        protected override SoilIndividualDayExposure createSoilIndividualExposure(
            IIndividualDay individualDay,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
