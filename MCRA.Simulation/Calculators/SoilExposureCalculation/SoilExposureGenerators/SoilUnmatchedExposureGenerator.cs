using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {

    public class SoilUnmatchedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and reference individuals
        /// </summary>
        protected override SoilIndividualDayExposure createSoilIndividualExposure(
            IIndividualDay individualDay,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, soilIndividualDayExposures.Count);
            var selected = soilIndividualDayExposures.ElementAt(ix);
            var result = selected.Clone();
            result.SimulatedIndividualId = individualDay.SimulatedIndividualId;
            result.SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId;
            result.IndividualSamplingWeight = individualDay.IndividualSamplingWeight;
            result.Individual = individualDay.Individual;
            result.Day = individualDay.Day;
            return result;
        }
    }
}
