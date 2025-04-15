using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.AirExposureGenerators {

    public class AirUnmatchedExposureGenerator : AirExposureGenerator {

        /// <summary>
        /// Randomly pair dust and reference individuals
        /// </summary>
        protected override AirIndividualDayExposure createAirIndividualExposure(
            IIndividualDay individualDay,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, airIndividualDayExposures.Count);
            var selected = airIndividualDayExposures.ElementAt(ix);
            var result = selected.Clone();
            result.SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId;
            result.SimulatedIndividual = individualDay.SimulatedIndividual;
            result.Day = individualDay.Day;
            return result;
        }
    }
}
