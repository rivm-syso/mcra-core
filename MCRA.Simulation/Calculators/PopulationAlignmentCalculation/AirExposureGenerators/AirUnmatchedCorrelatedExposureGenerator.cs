using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.AirExposureGenerators {

    public class AirUnmatchedCorrelatedExposureGenerator : AirExposureGenerator {

        /// <summary>
        /// Randomly pair air and dietary individuals
        /// (if the properties of the dietary individual match the properties of the air individual)
        /// </summary>
        protected override AirIndividualDayExposure createAirIndividualExposure(
            IIndividualDay individualDay,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
