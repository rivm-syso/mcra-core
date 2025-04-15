using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DustExposureGenerators {

    public class DustUnmatchedExposureGenerator : DustExposureGenerator {

        /// <summary>
        /// Randomly pair dust and reference individuals
        /// </summary>
        protected override DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, dustIndividualDayExposures.Count);
            var selected = dustIndividualDayExposures.ElementAt(ix);
            var result = selected.Clone();
            result.SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId;
            result.SimulatedIndividual = individualDay.SimulatedIndividual;
            result.Day = individualDay.Day;
            return result;
        }
    }
}
